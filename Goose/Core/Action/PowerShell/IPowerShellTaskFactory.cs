namespace Goose.Core.Action.PowerShell
{
    using System.Threading.Tasks;

    public interface IPowerShellTaskFactory
    {        
        Task Create(ShellCommand command);
    }
}