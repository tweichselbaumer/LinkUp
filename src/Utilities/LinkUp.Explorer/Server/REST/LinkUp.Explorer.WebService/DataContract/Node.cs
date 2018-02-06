using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp.Explorer.WebService.DataContract
{
    public class Node
    {
        private string _Name;
        public string Name { get => _Name; set => _Name = value; }
        public List<Node> Children { get => _Children; set => _Children = value; }
        public List<Label> Labels { get => _Labels; set => _Labels = value; }

        private List<Node> _Children = new List<Node>();

        private List<Label> _Labels = new List<Label>();
    }
}
