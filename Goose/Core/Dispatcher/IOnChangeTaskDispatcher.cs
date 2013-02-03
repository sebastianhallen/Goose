namespace Goose.Core.Dispatcher
{
    using System.Threading.Tasks;

    public interface IOnChangeTaskDispatcher
    {
        void QueueOnChangeTaskFor(string filePath);
        void QueueOnChangeTask(string projectPath, Task task);
    }
}
