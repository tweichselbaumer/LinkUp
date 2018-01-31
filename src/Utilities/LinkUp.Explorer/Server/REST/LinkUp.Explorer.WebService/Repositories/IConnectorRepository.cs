using LinkUp.Explorer.WebService.DataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp.Explorer.WebService.Repositories
{
    public interface IConnectorRepository
    {
        Connector Add(Connector connector);

        Connector Find(int id);

        IEnumerable<Connector> GetAll();

        Connector Remove(int id);

        void Update(Connector connector);
    }
}
