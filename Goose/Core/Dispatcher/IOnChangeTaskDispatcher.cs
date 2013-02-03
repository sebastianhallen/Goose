namespace Goose.Core.Dispatcher
{
    using Action;

    public interface IOnChangeTaskDispatcher
    {
        void QueueOnChangeTask(IGooseAction task);
    }
}
