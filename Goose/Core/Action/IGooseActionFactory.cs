namespace Goose.Core.Action
{
    using System.Collections.Generic;
    using Goose.Core.Configuration;

    public interface IGooseActionFactory
    {
        IEnumerable<IGooseAction> Create(ActionConfiguration configuration, IEnumerable<string> files);
    }
}