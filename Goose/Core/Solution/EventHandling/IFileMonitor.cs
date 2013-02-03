namespace Goose.Core.Solution.EventHandling
{
    using System.Threading.Tasks;

    public interface IFileMonitor
    {
        void MonitorProject(string projectPath, IGooseAction triggeredAction);
    }

    public interface IGooseAction
    {
        string Glob { get; }
        Task Work { get; }
    }
}