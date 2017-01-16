using LinkUp.Raw;
using System;

namespace LinkUp.Logic
{
    internal class LinkUpSubNode : IDisposable
    {
        private LinkUpConnector _Connector;

        internal LinkUpSubNode(LinkUpConnector connector)
        {
            _Connector = connector;
        }

        public LinkUpConnector Connector
        {
            get
            {
                return _Connector;
            }

            set
            {
                _Connector = value;
            }
        }

        public void Dispose()
        {
            Connector?.Dispose();
        }
    }
}