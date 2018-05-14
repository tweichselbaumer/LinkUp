using System.Collections.Generic;

namespace LinkUp.Explorer.WebService.DataContract
{
    public class Node
    {
        private List<Node> _Children = new List<Node>();
        private List<Label> _Labels = new List<Label>();
        private string _Name;
        public List<Node> Children { get => _Children; set => _Children = value; }
        public List<Label> Labels { get => _Labels; set => _Labels = value; }
        public string Name { get => _Name; set => _Name = value; }
    }
}