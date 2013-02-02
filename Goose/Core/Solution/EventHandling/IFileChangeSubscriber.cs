namespace Goose.Core.Solution.EventHandling
{
    public interface IFileChangeSubscriber
    {
        MonitoredFile Watch(string file);
        void Attach(IFileChangeConsumer fileMonitor);
    }
}