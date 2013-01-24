namespace Goose.Core.OnSaveTask
{
    using System.Threading.Tasks;

    public interface IOnSaveActionTaskFactory
    {
        Task CreateOnSaveAction(string projectDirectory);
    }
}
