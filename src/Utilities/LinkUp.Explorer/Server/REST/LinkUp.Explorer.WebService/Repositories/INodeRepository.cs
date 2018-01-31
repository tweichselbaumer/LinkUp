using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUp.Explorer.WebService.DataContract;

namespace LinkUp.Explorer.WebService.Repositories
{
    public interface INodeRepository
    {
        IEnumerable<WebService.DataContract.Node> GetAll();
    }
}
