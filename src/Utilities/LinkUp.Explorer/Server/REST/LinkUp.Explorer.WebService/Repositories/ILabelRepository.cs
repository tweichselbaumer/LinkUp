using LinkUp.Explorer.WebService.DataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp.Explorer.WebService.Repositories
{
    public interface ILabelRepository
    {
        Label GetLabel(string name);
    }
}
