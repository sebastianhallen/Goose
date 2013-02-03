namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    
    public interface IFileMonitor
    {
        void MonitorProject(string projectPath, string glob);
        void UnMonitor(IEnumerable<string> file);
        bool IsMonitoredProject(string project);
        bool IsMonitoredFile(string file);
    }
}