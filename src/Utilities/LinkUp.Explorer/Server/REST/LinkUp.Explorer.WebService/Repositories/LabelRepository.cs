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
            if (label is LinkUpPropertyLabelBase)
            {
                label.Value = (linkUpLabel as LinkUpPropertyLabelBase).ValueObject;
            }
            return label;
        }
    }
}