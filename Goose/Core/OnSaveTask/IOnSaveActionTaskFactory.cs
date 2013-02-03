namespace Goose.Core.OnSaveTask
{
    using System.Threading.Tasks;
    using Configuration;
    using Dispatcher;

    public interface IOnSaveActionTaskFactory
    {
        Task CreateOnSaveAction(string projectDirectory);
    }

    public interface IGooseActionFactory
    {
        GooseAction Create(ActionConfiguration configuration);
    }
}
