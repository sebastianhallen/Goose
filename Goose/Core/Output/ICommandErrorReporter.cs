namespace Goose.Core.Output
{
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;

    public interface ICommandErrorReporter
    {
        void Report(ShellCommand command, CommandResult result);
    }
}