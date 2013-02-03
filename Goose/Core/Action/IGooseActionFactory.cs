namespace Goose.Core.Action
{
    using Goose.Core.Configuration;

    public interface IGooseActionFactory
    {
        IGooseAction Create(ActionConfiguration configuration);
    }
}