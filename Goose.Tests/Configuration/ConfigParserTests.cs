namespace Goose.Tests.Configuration
{
    using System.IO;
    using System.Text;
    using Goose.Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigParserTests
    {
        private ActionConfigurationParser parser;

        [SetUp]
        public void Before()
        {
            this.parser = new ActionConfigurationParser();
        }

        [Test]
        public void Should_create_invalid_command_configuration_when_an_empty_action_tag_is_encountered()
        {
            var input = "<action></action>";

            var config = this.Parse(input);

            Assert.That(config.IsValid, Is.False);
        }

        [TestCase(null)]
        [TestCase("     ")]
        public void Should_not_explode_when_unable_to_read_config(string input)
        {
            this.parser.Parse(input);
        }

        [Test]
        public void Should_not_explode_when_unable_to_read_config_stream()
        {
            Stream stream = null;

            this.parser.Parse(stream);
        }

        [Test]
        public void Should_set_working_directory_when_a_working_directory_tag_is_present()
        {
            var input = "<action><working-directory>Build</working-directory></action>";

            var config = this.Parse(input);

            Assert.That(config.WorkingDirectory, Is.EqualTo("Build"));
        }

        [Test]
        public void Action_config_with_only_build_directory_is_not_value()
        {
            var input = "<action><working-directory>Build</working-directory></action>";

            var config = this.Parse(input);

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Command_should_be_set_when_a_command_node_is_present()
        {
            var input = "<action><command>some command</command></action>";

            var config = this.Parse(input);

            Assert.That(config.Command, Is.EqualTo("some command"));
        }

        [Test]
        public void action_config_with_only_command_should_not_be_valid()
        {
            var input = "<action><command>some command</command></action>";

            var config = this.Parse(input);

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Shell_should_be_powershell()
        {
            var input = "<action><command>some command</command></action>";

            var config = this.Parse(input);

            Assert.That(config.Shell, Is.EqualTo(Shell.PowerShell));
        }

        [Test]
        public void Should_set_action_trigger_attribute()
        {
            var input = @"<action on=""save""></action>";

            var config = this.Parse(input);

            Assert.That(config.Trigger, Is.EqualTo(Trigger.Save));
        }

        [Test]
        public void Should_set_trigger_to_unknown_when_not_specified()
        {
            var input = @"<action></action>";

            var config = this.Parse(input);

            Assert.That(config.Trigger, Is.EqualTo(Trigger.Unknown));
        }

        [Test]
        public void Should_set_trigger_to_unknown_when_unsupported_type_is_specified()
        {
            var input = @"<action on=""never""></action>";

            var config = this.Parse(input);

            Assert.That(config.Trigger, Is.EqualTo(Trigger.Unknown));
        }

        [Test]
        public void Should_be_able_to_parse_a_complete_action_configuration()
        {
            var input = @"
<action on=""save"">
  <working-directory>Build</working-directory>
  <command>$now = Get-Date ; Add-Content build.log ""Last build: $now""</command> 
</action>";

            var config = this.Parse(input);

            Assert.That(config.IsValid);
        }

        private ActionConfiguration Parse(string input)
        {
            return this.parser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(input)));
        }
    }
}
