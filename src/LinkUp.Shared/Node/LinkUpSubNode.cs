using LinkUp.Raw;
using System;
using System.Linq;

namespace LinkUp.Node
{
    internal class LinkUpSubNode : IDisposable
    {
        private LinkUpConnector _Connector;
        private bool _IsInitialized;
        private LinkUpNode _Master;
        private string _Name;
        private ushort _NextIdentifier = 1;

        internal LinkUpSubNode(LinkUpConnector connector, LinkUpNode master)
        {
            _Master = master;
            _Connector = connector;
            _Connector.ReveivedPacket += _Connector_ReveivedPacket;
        }

        public LinkUpConnector Connector
        {
            get
            {
                return _Connector;
            }
        }

        public void Dispose()
        {
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
                            LinkUpLabel label = _Master.AddSubLabel(nameRequest.Name, nameRequest.LabelType);
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