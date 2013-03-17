namespace Goose.Tests.Action
{
    using System.Linq;
    using Core.Action;
    using Core.Configuration;
    using FakeItEasy;
    using Goose.Core.Action.PowerShell;
    using NUnit.Framework;

    [TestFixture]
    public class ActionFactoryTests
    {
        [UnderTest] private PowerShellGooseActionFactory actionFactory;

        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
        }

        [Test]
        public void Should_not_return_any_actions_when_configuration_is_not_valid()
        {
            var actions = this.actionFactory.Create(new ActionConfiguration("root"), new[] { "first-file" });

            Assert.That(actions.Any(), Is.False);
        }

        [Test]
        public void Should_return_power_shell_action_when_configuration_is_valid()
        {
            var config = new ActionConfiguration(Trigger.Save, "glob", "", "ls", "root-directory", CommandScope.Project);

            var action = this.actionFactory.Create(config, new []{ "first-file" }).Single();

            Assert.That(action is PowerShellGooseAction);
        }

        [Test]
        public void Should_only_return_one_action_when_scope_is_per_project()
        {
            var config = new ActionConfiguration(Trigger.Save, "glob", "", "ls", "root-directory", CommandScope.Project);

            var actions = this.actionFactory.Create(config, new []{ "first-file", "second-file"});

            Assert.That(actions.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_return_one_action_per_input_file_when_scope_is_per_file()
        {
            var config = new ActionConfiguration(Trigger.Save, "glob", "", "ls", "root-directory", CommandScope.File);

            var actions = this.actionFactory.Create(config, new[] { "first-file", "second-file" });

            Assert.That(actions.Count(), Is.EqualTo(2));
        }
    }
}
