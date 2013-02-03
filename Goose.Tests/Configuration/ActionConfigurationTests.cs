namespace Goose.Tests.Configuration
{
    using Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ActionConfigurationTests
    {
        [Test]
        public void Should_set_project_path()
        {
            var config = new ActionConfiguration(Trigger.Save, "", "", "some-command", "project.csproj");
        
            Assert.That(config.ProjectRoot, Is.EqualTo("project.csproj"));
        }

        [Test]
        public void Save_configuration_with_non_empty_command_should_be_valid()
        {
            var config = new ActionConfiguration(Trigger.Save, "glob", "", "some-command", "project.csproj");

            Assert.That(config.IsValid);
        }

        [Test]
        public void Save_configuration_with_empty_command_should_not_be_valid()
        {
            var config = new ActionConfiguration(Trigger.Save, "", "", "", "project.csproj");

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Unknown_configuration_should_not_be_valid()
        {
            var config = new ActionConfiguration(Trigger.Unknown, "", "", "command", "project.csproj");

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Configuration_should_not_be_valid_when_working_directory_is_not_set()
        {
            var config = new ActionConfiguration(Trigger.Save, "", null, "command", "project.csproj");

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Configuration_should_not_be_valid_when_glob_is_empty()
        {
            var config = new ActionConfiguration(Trigger.Save, "", "", "command", "root");

            Assert.That(config.IsValid, Is.False);
        }
    }
}