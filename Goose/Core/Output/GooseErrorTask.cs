namespace Goose.Core.Output
{
    using Goose.Core.Action;
    using Microsoft.VisualStudio.Shell;

    public class GooseErrorTask
        : IGooseErrorTask
    {
        public GooseErrorTask(ShellCommand command, ErrorTask error)
        {
            this.Command = command;
            this.Error = error;
        }

        public ShellCommand Command { get; private set; }
        public ErrorTask Error { get; private set; }
    }
}