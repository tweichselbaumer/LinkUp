using LinkUp.Explorer.WebService.DataContract;

namespace LinkUp.Explorer.WebService.Repositories
{
    public interface ILabelRepository
    {
        Label GetLabel(string name);
    }
}