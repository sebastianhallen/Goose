namespace Goose.Core.Solution
{
    using Configuration;

    public interface IFileMonitor
    {
        void MonitorFile(FileInProject file, Trigger trigger);
        void MonitorProject(string path, ActionConfiguration watchConfiguration);
        bool IsMonitoredProject(string projectCandidate);
        void UpdateFileMonitorsForProject(string path);
    }
}