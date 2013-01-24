namespace Goose.Core.Dispatcher
{
    public interface IOnSaveTaskDispatcher
    {
        void QueueOnChangeTaskFor(string filePath);
    }
}
