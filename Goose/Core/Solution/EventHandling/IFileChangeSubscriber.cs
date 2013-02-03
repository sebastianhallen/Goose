namespace Goose.Core.Solution.EventHandling
{
    public interface IFileChangeSubscriber
    {
        MonitoredFile Subscribe(string project, string file);
        void UnSubscribe(uint cookie);
        void Attach(IFileChangeConsumer fileMonitor);
    }
}