using LinkUp.Raw;
using System;
using System.Linq;

#if NET45 || NETCOREAPP2_0
using System.Timers;
#endif

namespace LinkUp.Node
{
    public class LinkUpSubNode : IDisposable
    {
        private LinkUpConnector _Connector;
        private bool _IsInitialized;
        private LinkUpNode _Master;
        private string _Name;
        private ushort _NextIdentifier = 1;
        private int _LostPings = 0;
        private object _LockObject = new object();
#if NET45 || NETCOREAPP2_0
        private Timer _PingTimer;
#endif

        internal LinkUpSubNode(LinkUpConnector connector, LinkUpNode master)
        {
            _Master = master;
            _Connector = connector;
            _Connector.ReveivedPacket += _Connector_ReveivedPacket;
#if NET45 || NETCOREAPP2_0
            _PingTimer = new Timer(500);
            _PingTimer.Elapsed += _PingTimer_Elapsed;
            _PingTimer.Start();
#endif
        }

#if NET45 || NETCOREAPP2_0
        private void _PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_IsInitialized)
            {
                _Connector.SendPacket(new LinkUpPingRequest().ToPacket());
                _LostPings++;
            }
            if (_LostPings > 10)
            {
                if (_IsInitialized)
                    _Master.RemoveLabels(_Name);
                _IsInitialized = false;
            }
        }
#endif

        private ushort GetNextIdentifier()
        {
            lock (_LockObject)
            {
                return _NextIdentifier++;
            }

        }

        internal LinkUpConnector Connector
        {
            get
            {
                return _Connector;
            }
        }

        public void Dispose()
        {
#if NET45 || NETCOREAPP2_0
            _PingTimer.Stop();
#endif
            Connector?.Dispose();
        }

        internal void GetLabel<T>(LinkUpPrimitiveLabel<T> linkUpPrimitiveLabel)
#if NET45
where T : IConvertible, new()
#else
where T : new()
#endif
        {
            LinkUpPropertyGetRequest propertyGetRequest = new LinkUpPropertyGetRequest();
            propertyGetRequest.Identifier = linkUpPrimitiveLabel.ChildIdentifier;
            _Connector?.SendPacket(propertyGetRequest.ToPacket());
        }

        internal void SetLabel<T>(LinkUpPrimitiveLabel<T> linkUpPrimitiveLabel, byte[] data)
#if NET45
where T : IConvertible, new()
#else
where T : new()
#endif
        {
            LinkUpPropertySetRequest propertySetRequest = new LinkUpPropertySetRequest();
            propertySetRequest.Identifier = linkUpPrimitiveLabel.ChildIdentifier;
            propertySetRequest.Data = data;
            _Connector?.SendPacket(propertySetRequest.ToPacket());
        }

        private void _Connector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            try
            {
                LinkUpLogic logic = LinkUpLogic.ParseFromPacket(packet);
                if (logic is LinkUpNameRequest)
                {
                    LinkUpNameRequest nameRequest = logic as LinkUpNameRequest;
                    //if (!_IsInitialized && nameRequest.LabelType != LinkUpLabelType.Node)
                    //{
                    //    //TODO:ERROR??
                    //}
                    //else
                    //{
                        if (nameRequest.LabelType == LinkUpLabelType.Node)
                        {
                            _IsInitialized = true;
                            _Name = nameRequest.Name;

                            LinkUpNameResponse nameResponse = new LinkUpNameResponse();
                            nameResponse.Name = nameRequest.Name;
                            nameResponse.Identifier = 0;
                            nameResponse.LabelType = LinkUpLabelType.Node;
                            _Master.RemoveLabels(_Name);
                            _Connector.SendPacket(nameResponse.ToPacket());
                        }
                        if (nameRequest.LabelType != LinkUpLabelType.Node)
                        {
                            LinkUpLabel label = _Master.AddSubLabel(nameRequest.Name, nameRequest.LabelType);
                            if (label.ChildIdentifier == 0)
                            {
                                label.ChildIdentifier = GetNextIdentifier();
                            }
                            label.Owner = this;

                            LinkUpNameResponse nameResponse = new LinkUpNameResponse();
                            nameResponse.Name = nameRequest.Name;
                            nameResponse.Identifier = label.ChildIdentifier;
                            nameResponse.LabelType = nameRequest.LabelType;

                            _Connector.SendPacket(nameResponse.ToPacket());
                            Console.WriteLine("ID: {0} Name: {1}", nameResponse.Identifier, nameResponse.Name);
                        }
                    //}
                }
                else if (logic is LinkUpNameResponse)
                {
                    //TODO:ERROR??
                }
                else if (logic is LinkUpPropertyGetResponse)
                {
                    LinkUpPropertyGetResponse propertyGetResponse = (LinkUpPropertyGetResponse)logic;
                    LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.ChildIdentifier == propertyGetResponse.Identifier);
                    if (label != null)
                    {
                        if (label is LinkUpPrimitiveBaseLabel)
                        {
                            (label as LinkUpPrimitiveBaseLabel).GetDone(propertyGetResponse.Data);
                        }
                    }
                }
                else if (logic is LinkUpPropertySetResponse)
                {
                    LinkUpPropertySetResponse propertySetResponse = (LinkUpPropertySetResponse)logic;
                    LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.ChildIdentifier == propertySetResponse.Identifier);
                    if (label != null)
                    {
                        if (label is LinkUpPrimitiveBaseLabel)
                        {
                            (label as LinkUpPrimitiveBaseLabel).SetDone();
                        }
                    }
                }
                else if (logic is LinkUpPingResponse)
                {
                    _LostPings = 0;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}