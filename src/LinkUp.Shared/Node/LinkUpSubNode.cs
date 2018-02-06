using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace LinkUp.Node
{
    public class LinkUpSubNode : IDisposable
    {
        private LinkUpConnector _Connector;
        private bool _IsInitialized;
        private LinkUpNode _Master;
        private string _Name;
        private ushort _NextIdentifier = 1;
#if NET45 || NETCOREAPP2_0
        private Timer _PingTimer;
#endif

        internal LinkUpSubNode(LinkUpConnector connector, LinkUpNode master)
        {
            _Master = master;
            _Connector = connector;
            _Connector.ReveivedPacket += _Connector_ReveivedPacket;
#if NET45 || NETCOREAPP2_0
            _PingTimer = new Timer(1000);
            _PingTimer.Elapsed += _PingTimer_Elapsed;
            _PingTimer.Start();
#endif
        }

#if NET45 || NETCOREAPP2_0
        private void _PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_IsInitialized)
                _Connector.SendPacket(new LinkUpPingRequest().ToPacket());
        }
#endif
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
                    if (!_IsInitialized && nameRequest.LabelType != LinkUpLabelType.Node)
                    {
                        //TODO:ERROR??
                    }
                    else
                    {
                        if (nameRequest.LabelType == LinkUpLabelType.Node)
                        {
                            _IsInitialized = true;
                            _Name = nameRequest.Name;

                            LinkUpNameResponse nameResponse = new LinkUpNameResponse();
                            nameResponse.Name = nameRequest.Name;
                            nameResponse.Identifier = 0;
                            nameResponse.LabelType = LinkUpLabelType.Node;
                            //TODO: remove old labels
                            _Connector.SendPacket(nameResponse.ToPacket());
                        }
                        if (nameRequest.LabelType != LinkUpLabelType.Node)
                        {
                            LinkUpNameResponse nameResponse = new LinkUpNameResponse();
                            nameResponse.Name = nameRequest.Name;
                            nameResponse.Identifier = _NextIdentifier++;
                            nameResponse.LabelType = nameRequest.LabelType;
                            _Connector.SendPacket(nameResponse.ToPacket());
                            LinkUpLabel label = _Master.AddSubLabel(string.Format("{0}/{1}", _Name, nameRequest.Name), nameRequest.LabelType);
                            label.Owner = this;
                            label.ChildIdentifier = nameResponse.Identifier;
                        }
                    }
                }
                if (logic is LinkUpNameResponse)
                {
                    //TODO:ERROR??
                }
                if (logic is LinkUpPropertyGetResponse)
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
                if (logic is LinkUpPropertySetResponse)
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
            }
            catch (Exception)
            {
            }
        }
    }
}