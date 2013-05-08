namespace Goose.Core.Action.PowerShell
{
    public interface IShellCommandRunner
    {
        CommandResult RunCommand(ShellCommand command);
    }
}