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
        public DataContract.Node GetAll()
        {
            DataContract.Node node = new DataContract.Node();
            node.Name = _Node.Name;

            foreach (LinkUpLabel label in _Node.Labels)
            {
                AddNode(label.Name.Split('/').Skip(1).ToList(), node);
            }
            return node;
        }

        private void AddNode(List<string> labelnames, DataContract.Node parent)
        {
            if (labelnames.Count > 1)
            {
                DataContract.Node node;
                if (!parent.Children.Any(c => c.Name == labelnames[0]))
                {
                    node = new DataContract.Node();
                    node.Name = labelnames[0];
                    parent.Children.Add(node);
                }
                else
                {
                    node = parent.Children.FirstOrDefault(c => c.Name == labelnames[0]);
                }
                AddNode(labelnames.Skip(1).ToList(), node);
            }
            else
            {
                Label label = new Label();
                label.Name = labelnames[0];
                parent.Labels.Add(label);
            }
        }
    }
}
