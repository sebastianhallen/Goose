namespace Goose.Core.Dispatcher
{
    using Action;

    public interface IOnChangeTaskDispatcher
    {
        void QueueOnChangeTaskFor(string filePath);
        void QueueOnChangeTask(GooseAction task);
    }
}
