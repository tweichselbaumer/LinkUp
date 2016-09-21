using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUp.Portable
{
    public class LinkUpBridge : IDisposable
    {
        private List<LinkUpConnector> _Connectors = new List<LinkUpConnector>();

        public event ReveicedPacketEventHandler ReceivedPacket;

        public void AddConnector(LinkUpConnector connector)
        {
            _Connectors.Add(connector);
            connector.ReveivedPacket += Connector_ReveivedPacket;
        }

        public void Dispose()
        {
            if (_Connectors != null)
            {
                foreach (LinkUpConnector connector in _Connectors)
                {
                    connector.Dispose();
                }
            }
        }

        public void Send(LinkUpPacket packet)
        {
            if (_Connectors != null)
            {
                foreach (LinkUpConnector connector in _Connectors)
                {
                    connector.SendPacket(packet);
                }
            }
        }

        private void Connector_ReveivedPacket(LinkUpConnector sender, LinkUpPacket packet)
        {
            ReceivedPacket?.Invoke(sender, packet);

            if (_Connectors != null)
            {
                foreach (LinkUpConnector connector in _Connectors.Where(c => c != sender))
                {
                    connector.SendPacket(packet);
                }
            }
        }
    }
}