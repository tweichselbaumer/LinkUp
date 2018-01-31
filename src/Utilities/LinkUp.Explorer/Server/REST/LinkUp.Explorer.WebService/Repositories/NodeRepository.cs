using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUp.Explorer.WebService.DataContract;
using LinkUp.Node;

namespace LinkUp.Explorer.WebService.Repositories
{
    public class NodeRepository : INodeRepository
    {

        private LinkUpNode _Node;

        public NodeRepository(LinkUpNode node)
        {
            _Node = node;
        }
        public IEnumerable<DataContract.Node> GetAll()
        {
            return Enumerable.Range(1, 5).Select(c => new DataContract.Node() { Name = string.Format("Node #{0}", c) }).ToList();
        }
    }
}
