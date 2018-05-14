using LinkUp.Explorer.WebService.DataContract;
using LinkUp.Node;
using System.Linq;

namespace LinkUp.Explorer.WebService.Repositories
{
    public class LabelRepository : ILabelRepository
    {
        private LinkUpNode _Node;

        public LabelRepository(LinkUpNode node)
        {
            _Node = node;
        }

        public Label GetLabel(string name)
        {
            LinkUpLabel linkUpLabel = _Node.Labels.FirstOrDefault(c => c.Name.Equals(name));
            Label label = new Label();
            label.Name = linkUpLabel.Name;
            label.Value = linkUpLabel.ValueObject;
            return label;
        }
    }
}