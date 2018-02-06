using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        }

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

        internal LinkUpLabel AddSubLabel(string name, LinkUpLabelType type)
        {
            LinkUpLabel label = LinkUpLabel.CreateNew(type);
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
            lock (_MasterConnector)
            {
                MasterConnector?.Dispose();
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
                if (logic is LinkUpNameResponse)
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
                if (logic is LinkUpPropertyGetRequest)
                {
                    LinkUpPropertyGetRequest propertyGetRequest = (LinkUpPropertyGetRequest)logic;
                    LinkUpPropertyGetResponse propertyGetResponse = new LinkUpPropertyGetResponse();
                    LinkUpLabel label = _Labels.FirstOrDefault(c => c.ParentIdentifier == propertyGetRequest.Identifier);
                    if (label != null && label is LinkUpPrimitiveBaseLabel)
                    {
                        propertyGetResponse.Identifier = label.ParentIdentifier;
                        propertyGetResponse.Data = (label as LinkUpPrimitiveBaseLabel).Data;
                        connector.SendPacket(propertyGetResponse.ToPacket());
                    }
                    else
                    {
                        //TODO: ERROR?
                    }
                }
                if (logic is LinkUpPropertySetRequest)
                {
                    LinkUpPropertySetRequest propertySetRequest = (LinkUpPropertySetRequest)logic;
                    LinkUpPropertySetResponse propertyGetResponse = new LinkUpPropertySetResponse();
                    LinkUpLabel label = _Labels.FirstOrDefault(c => c.ParentIdentifier == propertySetRequest.Identifier);
                    if (label != null && label is LinkUpPrimitiveBaseLabel)
                    {
                        (label as LinkUpPrimitiveBaseLabel).Data = propertySetRequest.Data;
                        propertyGetResponse.Identifier = label.ParentIdentifier;
                        connector.SendPacket(propertyGetResponse.ToPacket());
                    }
                    else
                    {
                        //TODO: ERROR?
                    }
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