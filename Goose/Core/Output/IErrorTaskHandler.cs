namespace Goose.Core.Output
{
    using System.Collections.Generic;
    using Goose.Core.Action;

    public interface IErrorTaskHandler
    {
        void Add(IEnumerable<IGooseErrorTask> task);
        void Remove(ShellCommand command);
        IEnumerable<IGooseErrorTask> Existing(ShellCommand command);
    }
}