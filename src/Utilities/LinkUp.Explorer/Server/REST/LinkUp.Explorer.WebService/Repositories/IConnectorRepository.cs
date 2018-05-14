using LinkUp.Explorer.WebService.DataContract;
using System.Collections.Generic;

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