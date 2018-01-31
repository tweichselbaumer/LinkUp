using LinkUp.Explorer.WebService.DataContract;
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

        public Connector Add(Connector connector)
        {
            throw new NotImplementedException();
        }

        public Connector Find(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Connector> GetAll()
        {
            return Enumerable.Range(1, 5).Select(c => new Connector() { Name = string.Format("Connector #{0}", c) }).ToList();
        }

        public Connector Remove(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(Connector connector)
        {
            throw new NotImplementedException();
        }
    }
}
