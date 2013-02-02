namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    using Configuration;
    using Microsoft.VisualStudio.Shell.Interop;

    public interface IFileChangeConsumer
        : IVsFileChangeEvents
    {
        void ActOn(IEnumerable<string> files, Trigger trigger);
    }
}