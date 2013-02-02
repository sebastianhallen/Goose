namespace Goose.Core.Solution
{    
    public interface IFileMonitor
    {
        void MonitorFile(FileInProject file);
        void MonitorProject(string path);
    }
}