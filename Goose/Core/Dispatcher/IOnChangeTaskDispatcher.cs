namespace Goose.Core.Dispatcher
{
    using OnSaveTask;

    public interface IOnChangeTaskDispatcher
    {
        void QueueOnChangeTaskFor(string filePath);
        void QueueOnChangeTask(GooseAction task);
    }
}
