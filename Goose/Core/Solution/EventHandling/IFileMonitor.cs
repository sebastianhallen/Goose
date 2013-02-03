namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFileMonitor
    {
        void MonitorProject(string projectPath, IGooseAction triggeredAction);
        void UnMonitor(IEnumerable<string> file);
    }

    public interface IGooseAction
    {
        string Glob { get; }
        Task Work { get; }
    }
}