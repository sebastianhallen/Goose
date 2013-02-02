namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    using Configuration;

    public interface IFileChangeConsumer
    {
        void ActOn(IEnumerable<uint> cookies, Trigger trigger);
    }
}