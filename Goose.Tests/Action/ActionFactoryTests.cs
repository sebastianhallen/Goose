namespace Goose.Tests.Action
{
    using System.Linq;
    using Core.Configuration;
    using FakeItEasy;
    using Goose.Core.Action.PowerShell;
    using NUnit.Framework;

    [TestFixture]
    public class ActionFactoryTests
    {
        [UnderTest] private PowerShellGooseActionFactory actionFactory;
        private ActionConfigurationBuilder configBuilder;

        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
            this.configBuilder = new ActionConfigurationBuilder();
        }

        [Test]
        public void Should_not_return_any_actions_when_configuration_is_not_valid()
        {
            var config = this.configBuilder.Build();

            var actions = this.actionFactory.Create(config, new[] { "first-file" });

            Assert.That(actions.Any(), Is.False);
        }

        [Test]
        public void Should_return_power_shell_action_when_configuration_is_valid()
        {

            var config = this.configBuilder
                             .On(Trigger.Save).FilesMatching("glob")
                             .Run("ls").In("working-directory").ForProjectIn("root-directory")
                             .WithScope(CommandScope.Project)
                             .Build();

            var action = this.actionFactory.Create(config, new []{ "first-file" }).Single();

            Assert.That(action is PowerShellGooseAction);
        }

        [Test]
        public void Should_only_return_one_action_when_scope_is_per_project()
        {
            var config = this.configBuilder
                             .On(Trigger.Save).FilesMatching("glob")
                             .Run("ls").In("working-directory").ForProjectIn("root-directory")
                             .WithScope(CommandScope.Project)
                             .Build();

            var actions = this.actionFactory.Create(config, new []{ "first-file", "second-file"});

            Assert.That(actions.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_return_one_action_per_input_file_when_scope_is_per_file()
        {
            var config = this.configBuilder
                             .On(Trigger.Save).FilesMatching("glob")
                             .Run("ls").In("working-directory").ForProjectIn("root-directory")
                             .WithScope(CommandScope.File)
                             .Build();

            var actions = this.actionFactory.Create(config, new[] { "first-file", "second-file" });

            Assert.That(actions.Count(), Is.EqualTo(2));
        }
    }
}
