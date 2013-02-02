namespace Goose.Core.Solution.EventHandling
{
    using Goose.Core.Configuration;

    public interface IFileMonitor
    {
        void MonitorProject(string path, ActionConfiguration watchConfiguration);
    }
}