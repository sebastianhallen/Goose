namespace Goose.Tests.EventHandling
{
    using Core.Configuration;
    using FakeItEasy;

    public class ActionConfigurationDummy
        : DummyDefinition<ActionConfiguration>
    {
        protected override ActionConfiguration CreateDummy()
        {
            return new ActionConfigurationBuilder()
                .ForProjectIn("project-root").ProjectInSolution("solution-root")
                .On(Trigger.Save).FilesMatching("*.less").Run("command").WithScope(CommandScope.Project)
                .In("")
            .Build();
        }
    }
}