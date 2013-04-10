namespace Goose.Core.Output
{
    using Goose.Core.Action;
    using Microsoft.VisualStudio.Shell;

    public interface IGooseErrorTask
    {
        ShellCommand Command { get; }
        ErrorTask Error { get; }
    }
}