﻿using LinkUp.Raw;
using System;
using System.Linq;

namespace LinkUp.Logic
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
            propertyGetRequest.Identifier = linkUpPrimitiveLabel.Identifier;
            _Connector?.SendPacket(propertyGetRequest.ToPacket());
        }

        internal void SetLabel<T>(LinkUpPrimitiveLabel<T> linkUpPrimitiveLabel, byte[] data)
#if NET45
where T : IConvertible, new()
#else
where T : new()
#endif
        {
            throw new NotImplementedException();
        }

        private void _Connector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Console.WriteLine("MASTER: " + _Name + " - " + DateTime.Now);
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
                    LinkUpLabel label = _Master.Labels.FirstOrDefault(c => c.Identifier == propertyGetResponse.Identifier);
                    if (label != null)
                    {
                        if (label is LinkUpPrimitiveBaseLabel)
                        {
                            (label as LinkUpPrimitiveBaseLabel).Data = propertyGetResponse.Data;
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