namespace Goose.Core.Solution
{
    using Configuration;

    public interface IFileMonitor
    {
        void MonitorFile(FileInProject file, Trigger trigger);
        void MonitorProject(string path);
    }
}