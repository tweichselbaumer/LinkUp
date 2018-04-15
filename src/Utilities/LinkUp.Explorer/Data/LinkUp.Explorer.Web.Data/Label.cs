using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp.Explorer.WebService.DataContract
{
    public class Label
    {
        private string _Name;
        public string Name { get => _Name; set => _Name = value; }
        public object Value { get => _Value; set => _Value = value; }

        private object _Value;

    }
}
