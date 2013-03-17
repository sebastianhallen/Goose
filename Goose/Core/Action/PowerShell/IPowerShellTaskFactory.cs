namespace Goose.Core.Action.PowerShell
{
    using System.Threading.Tasks;

    public interface IPowerShellTaskFactory
    {
        Task Create(string command);
    }
}