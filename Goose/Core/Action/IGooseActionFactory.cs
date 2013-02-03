namespace Goose.Core.Action
{
    using Goose.Core.Configuration;

    public interface IGooseActionFactory
    {
        GooseAction Create(ActionConfiguration configuration);
    }
}