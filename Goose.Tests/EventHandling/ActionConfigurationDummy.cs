namespace Goose.Tests.EventHandling
{
    using Core.Configuration;
    using FakeItEasy;

    public class ActionConfigurationDummy
        : DummyDefinition<ActionConfiguration>
    {
        protected override ActionConfiguration CreateDummy()
        {
            return new ActionConfiguration(Trigger.Save, "", "command", "root");
        }
    }
}