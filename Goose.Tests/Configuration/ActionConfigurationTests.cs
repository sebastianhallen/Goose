namespace Goose.Tests.Configuration
{
    using Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ActionConfigurationTests
    {
        [Test]
        public void Save_configuration_with_non_empty_command_should_be_valid()
        {
            var config = new ActionConfiguration(Trigger.Save, "", "some-command");

            Assert.That(config.IsValid);
        }

        [Test]
        public void Save_configuration_with_empty_command_should_not_be_valid()
        {
            var config = new ActionConfiguration(Trigger.Save, "", "");

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Unknown_configuration_should_not_be_valid()
        {
            var config = new ActionConfiguration(Trigger.Unknown, "", "command");

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Configuration_should_not_be_valid_when_working_directory_is_not_set()
        {
            var config = new ActionConfiguration(Trigger.Save, null, "command");

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Glob_should_be_less()
        {
            var config = new ActionConfiguration();

            Assert.That(config.Glob, Is.EqualTo("*.less"));
        }
    }
}