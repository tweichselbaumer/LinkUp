using LinkUp.Raw;
using System;
using System.Collections.Generic;

namespace LinkUp.Logic
{
    public class LinkUpNode : IDisposable
    {
        private LinkUpConnector _MasterConnector;
        private string _Name;
        private List<LinkUpSubNode> _SubNodes = new List<LinkUpSubNode>();

        public LinkUpConnector MasterConnector
        {
            get
            {
                return _MasterConnector;
            }

            set
            {
                value.ReveivedPacket += MasterConnector_ReveivedPacket;
                _MasterConnector = value;
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
                _Name = value;
            }
        }

        public void AddSubNode(LinkUpConnector connector)
        {
            _SubNodes.Add(new LinkUpSubNode(connector));
        }

        public void Dispose()
        {
            if (_SubNodes != null)
            {
                foreach (LinkUpSubNode subnode in _SubNodes)
                {
                    subnode?.Dispose();
                }
            }

            MasterConnector?.Dispose();
        }

        private void MasterConnector_ReveivedPacket(LinkUpConnector sender, LinkUpPacket packet)
        {
        }
    }
}