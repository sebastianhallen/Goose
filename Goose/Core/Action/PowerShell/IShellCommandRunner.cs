namespace Goose.Core.Action.PowerShell
{
    public interface IShellCommandRunner
    {
        string RunCommand(ShellCommand command);
    }
}