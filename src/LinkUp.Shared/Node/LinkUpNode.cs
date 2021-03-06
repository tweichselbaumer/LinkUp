﻿using LinkUp.Node.Logic;
using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Node
{
    public class LinkUpNode : IDisposable
    {
        private const int NAME_REQUEST_TIMEOUT = 1000;
        private ushort _Identifier;
        private bool _IsInitialized;
        private bool _IsRunning = true;
        private List<LinkUpLabel> _Labels = new List<LinkUpLabel>();
        private DateTime _LastUpdate;
        private LinkUpConnector _MasterConnector;
        private string _Name;
        private List<LinkUpSubNode> _SubNodes = new List<LinkUpSubNode>();
        private Task _Task;
        private AutoResetEvent _Event;
        private int _LostPings = 0;

#if NET45 || NETCOREAPP2_0
        private System.Timers.Timer _PingTimer;
#endif

        public LinkUpNode()
        {
            _Event = new AutoResetEvent(false);
            _Task = Task.Run(() =>
            {
                while (_IsRunning)
                {
                    _Event.Reset();
                    UpdateNames();
                    _Event.WaitOne(1000);
                }
            });
#if NET45 || NETCOREAPP2_0
            _PingTimer = new System.Timers.Timer(500);
            _PingTimer.Elapsed += _PingTimer_Elapsed;
            _PingTimer.Start();
#endif
        }

#if NET45 || NETCOREAPP2_0

        private void _PingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_IsInitialized)
            {
                _LostPings++;
            }
            if (_LostPings > 10)
            {
                _IsInitialized = false;
                lock (_Labels)
                {
                    foreach (LinkUpLabel label in _Labels)
                    {
                        label.IsInitialized = false;
                    }
                }
            }
        }

#endif

        public List<LinkUpLabel> Labels
        {
            get
            {
                List<LinkUpLabel> result;
                lock (_Labels)
                {
                    result = _Labels.ToList();
                }
                return result;
            }
        }

        public LinkUpConnector MasterConnector
        {
            get
            {
                return _MasterConnector;
            }

            set
            {
                if (_MasterConnector != null)
                {
                    _MasterConnector.ReveivedPacket -= MasterConnector_ReveivedPacket;
                }

                value.ReveivedPacket += MasterConnector_ReveivedPacket;

                _MasterConnector = value;
                _IsInitialized = false;
                foreach (LinkUpLabel label in Labels)
                {
                    label.IsInitialized = false;
                }
                _Event.Set();
            }
        }

        public T GetLabelByName<T>(string name) where T : LinkUpLabel, new()
        {
            lock (_Labels)
            {
                LinkUpLabel label = _Labels.FirstOrDefault(c => c.Name.Equals(name) && c is T);
                if (label != null)
                {
                    return (T)label;
                }
                else
                {
                    if (_Labels.Any(c => c.Name.Equals(name)))
                    {
                        throw new Exception(string.Format("Label {0} has wrong type.", name));
                    }
                    else
                    {
                        label = new T();
                        label.Name = name;
                        _Labels.Add(label);

                        return (T)label;
                    }
                }
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                if (!value.Cast<char>().All(c => char.IsLetterOrDigit(c)))
                {
                    throw new Exception("Only letters and digits are allowed for the LinkUpNode name!");
                }
                _Name = value;
                _IsInitialized = false;
                _Event.Set();
            }
        }

        public List<LinkUpSubNode> SubNodes { get => _SubNodes; }

        public T AddLabel<T>(string name) where T : LinkUpLabel, new()
        {
            T label = new T();
            if (!name.Cast<char>().All(c => char.IsLetterOrDigit(c)))
            {
                throw new Exception("Only letters and digits are allowed for the LinkUpLabel name!");
            }
            label.Name = string.Format("{0}/{1}", Name, name);
            if (Labels.Any(c => c.Name == label.Name))
            {
                throw new Exception("Label with specified name already exists!");
            }
            lock (_Labels)
            {
                _Labels.Add(label);
            }
            _Event.Set();
            return label;
        }

        public void RemoveLabel(string name)
        {
            string fullName = string.Format("{0}/{1}", Name, name);
            if (Labels.Any(c => c.Name == fullName))
            {
                _Labels.RemoveAll(c => c.Name == fullName);
            }
            else
            {
                throw new Exception("Label with specified name doesn't exists!");
            }
        }

        internal void RemoveLabels(string name)
        {
            string pattern = string.Format("{0}\\/{1}\\/[\\S]{{1,}}", _Name, name);
            _Labels.RemoveAll(c => Regex.Match(c.Name, pattern).Success);
        }

        internal LinkUpLabel AddSubLabel(string name, LinkUpLabelType type, byte[] options)
        {
            LinkUpLabel label;
            string labelName = string.Format("{0}/{1}", Name, name);
            lock (_Labels)
            {
                if (_Labels.Any(c => c.Name == labelName))
                {
                    label = _Labels.FirstOrDefault(c => c.Name == labelName);
                    if (label.LabelType != type)
                    {
                        _Labels.Remove(label);
                        label = LinkUpLabel.CreateNew(type, options);
                        label.Name = labelName;
                        _Labels.Add(label);
                    }
                }
                else
                {
                    label = LinkUpLabel.CreateNew(type, options);
                    label.Name = labelName;
                    _Labels.Add(label);
                }
            }
            _Event.Set();
            return label;
        }

        public void AddSubNode(LinkUpConnector connector)
        {
            SubNodes.Add(new LinkUpSubNode(connector, this));
        }

        public void Dispose()
        {
            if (_Task != null && _Task.Status == TaskStatus.Running)
            {
                _IsRunning = false;
                _Event.Set();
                _Task.Wait();
            }
            lock (SubNodes)
            {
                if (SubNodes != null)
                {
                    foreach (LinkUpSubNode subnode in SubNodes)
                    {
                        subnode?.Dispose();
                    }
                }
            }
            if (_MasterConnector != null)
            {
                lock (_MasterConnector)
                {
                    MasterConnector?.Dispose();
                }
            }
        }

        private void MasterConnector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            try
            {
                LinkUpLogic logic = LinkUpLogic.ParseFromPacket(packet);
                if (logic is LinkUpNameRequest)
                {
                    //TODO:ERROR??
                }
                else if (logic is LinkUpNameResponse)
                {
                    LinkUpNameResponse nameResponse = logic as LinkUpNameResponse;
                    LinkUpLabel label = _Labels.FirstOrDefault(c => c.Name == nameResponse.Name);
                    if (label != null && nameResponse.LabelType != LinkUpLabelType.Node)
                    {
                        label.ParentIdentifier = nameResponse.Identifier;
                        label.IsInitialized = true;
                    }
                    else if (nameResponse.Name == _Name && nameResponse.LabelType == LinkUpLabelType.Node)
                    {
                        _Identifier = (logic as LinkUpNameResponse).Identifier;
                        _IsInitialized = true;
                    }
                }
                else if (logic is LinkUpPropertyGetRequest)
                {
                    LinkUpPropertyGetRequest propertyGetRequest = (LinkUpPropertyGetRequest)logic;
                    LinkUpPropertyGetResponse propertyGetResponse = new LinkUpPropertyGetResponse();
                    LinkUpLabel label = _Labels.FirstOrDefault(c => c.ParentIdentifier == propertyGetRequest.Identifier);
                    if (label != null && label is LinkUpPropertyLabelBase)
                    {
                        propertyGetResponse.Identifier = label.ParentIdentifier;
                        propertyGetResponse.Data = (label as LinkUpPropertyLabelBase).Data;
                        connector.SendPacket(propertyGetResponse.ToPacket());
                    }
                    else
                    {
                        //TODO: ERROR?
                    }
                }
                else if (logic is LinkUpPropertySetRequest)
                {
                    LinkUpPropertySetRequest propertySetRequest = (LinkUpPropertySetRequest)logic;
                    LinkUpPropertySetResponse propertyGetResponse = new LinkUpPropertySetResponse();
                    LinkUpLabel label = _Labels.FirstOrDefault(c => c.ParentIdentifier == propertySetRequest.Identifier);
                    if (label != null && label is LinkUpPropertyLabelBase)
                    {
                        (label as LinkUpPropertyLabelBase).Data = propertySetRequest.Data;
                        propertyGetResponse.Identifier = label.ParentIdentifier;
                        connector.SendPacket(propertyGetResponse.ToPacket());
                    }
                    else
                    {
                        //TODO: ERROR?
                    }
                }
                else if (logic is LinkUpPingRequest)
                {
                    connector.SendPacket(new LinkUpPingResponse().ToPacket());
                    _LostPings = 0;
                }
                else if (logic is LinkUpEventFireResponse)
                {
                    //TODO:
                }
                else if (logic is LinkUpEventSubscribeRequest)
                {
                    //TODO:
                }
                else if (logic is LinkUpEventUnsubscribeRequest)
                {
                    //TODO:
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateNames()
        {
            if (_MasterConnector != null)
            {
                lock (_MasterConnector)
                {
                    if (!_MasterConnector.IsDisposed)
                    {
                        if (!_IsInitialized && _LastUpdate.AddMilliseconds(NAME_REQUEST_TIMEOUT) < DateTime.Now && _Name != null)
                        {
                            LinkUpNameRequest nameRequest = new LinkUpNameRequest();
                            nameRequest.Name = _Name;
                            nameRequest.LabelType = LinkUpLabelType.Node;
                            _MasterConnector.SendPacket(nameRequest.ToPacket());
                            _LastUpdate = DateTime.Now;
                        }
                        if (_IsInitialized)
                        {
                            lock (_Labels)
                            {
                                foreach (LinkUpLabel label in _Labels.Where(c => !c.IsInitialized && c.LastUpdate.AddMilliseconds(NAME_REQUEST_TIMEOUT) < DateTime.Now))
                                {
                                    LinkUpNameRequest nameRequest = new LinkUpNameRequest();
                                    nameRequest.Name = label.Name;
                                    nameRequest.LabelType = label.LabelType;
                                    _MasterConnector.SendPacket(nameRequest.ToPacket());
                                    label.LastUpdate = DateTime.Now;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}