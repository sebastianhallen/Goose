namespace Goose.Core.Solution.EventHandling
{
    using System;
    using System.Collections.Generic;
    
    public interface IFileMonitor
        : IDisposable
    {
        void MonitorProject(string projectPath, string glob);
        void UnMonitor(IEnumerable<string> file);
        bool IsMonitoredProject(string project);
        bool IsMonitoredFile(string file);
    }
}