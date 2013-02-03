namespace Goose.Core.Action
{
    using System.Threading.Tasks;

    public interface IOnSaveActionTaskFactory
    {
        Task CreateOnSaveAction(string projectDirectory);
    }
}
