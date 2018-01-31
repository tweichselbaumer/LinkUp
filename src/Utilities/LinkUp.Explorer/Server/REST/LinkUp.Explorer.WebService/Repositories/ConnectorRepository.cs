using LinkUp.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp.Explorer.WebService.Repositories
{
    public class ConnectorRepository : IConnectorRepository
    {
        private LinkUpNode _Node;

        public ConnectorRepository(LinkUpNode node)
        {
            _Node = node;
        }
    }
}
