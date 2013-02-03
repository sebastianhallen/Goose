namespace Goose.Tests.Action
{
    using Core.Action;
    using Core.Configuration;
    using FakeItEasy;
    using NUnit.Framework;

    [TestFixture]
    public class ActionFactoryTests
    {
        [UnderTest] private GooseActionFactory actionFactory;

        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
        }

        [Test]
        public void Should_return_void_action_when_configuration_is_not_valid()
        {
            var action = this.actionFactory.Create(new ActionConfiguration("root"));

            Assert.That(action is VoidGooseAction);
        }

        [Test]
        public void Should_return_power_shell_action_when_configuration_is_valid()
        {
            var action = this.actionFactory.Create(new ActionConfiguration(Trigger.Save, "", "ls", "root-directory"));

            Assert.That(action is PowerShellGooseAction);
        }


    }
}
