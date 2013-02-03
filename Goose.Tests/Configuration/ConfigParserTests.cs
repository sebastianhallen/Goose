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
            var input = @"<goose version=""1.0""><action></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.IsValid, Is.False);
        }

        [TestCase(null)]
        [TestCase("     ")]
        public void Should_not_explode_when_unable_to_read_config(string input)
        {
            this.parser.Parse("", input);
        }

        [Test]
        public void Should_not_explode_when_unable_to_read_config_stream()
        {
            Stream stream = null;

            this.parser.Parse("", stream);
        }

        [Test]
        public void Should_set_working_directory_when_a_working_directory_tag_is_present()
        {
            var input = @"<goose version=""1.0""><action><working-directory>Build</working-directory></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.WorkingDirectory, Is.EqualTo("Build"));
        }

        [Test]
        public void Action_config_with_only_build_directory_is_not_value()
        {
            var input = @"<goose version=""1.0""><action><working-directory>Build</working-directory></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Command_should_be_set_when_a_command_node_is_present()
        {
            var input = @"<goose version=""1.0""><action><command>some command</command></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.Command, Is.EqualTo("some command"));
        }

        [Test]
        public void action_config_with_only_command_should_not_be_valid()
        {
            var input = @"<goose version=""1.0""><action><command>some command</command></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Shell_should_be_powershell()
        {
            var input = @"<goose version=""1.0""><action><command>some command</command></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.Shell, Is.EqualTo(Shell.PowerShell));
        }

        [Test]
        public void Should_set_action_trigger_attribute()
        {
            var input = @"<goose version=""1.0""><action on=""save""></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.Trigger, Is.EqualTo(Trigger.Save));
        }

        [Test]
        public void Should_set_trigger_to_unknown_when_not_specified()
        {
            var input = @"<goose version=""1.0""><action></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.Trigger, Is.EqualTo(Trigger.Unknown));
        }

        [Test]
        public void Should_set_trigger_to_unknown_when_unsupported_type_is_specified()
        {
            var input = @"<goose version=""1.0""><action on=""never""></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.Trigger, Is.EqualTo(Trigger.Unknown));
        }

        [Test]
        public void Should_set_glob_to_default_value_when_no_glob_is_present()
        {
            var input = @"<goose version=""1.0""><action glob=""*.ext""></action></goose>";

            var config = this.Parse(input);

            Assert.That(config.Glob, Is.EqualTo("*.ext"));
        }

        [Test]
        public void Should_be_able_to_parse_a_complete_action_configuration()
        {
            var input = @"
<goose version=""1.0"">
    <action on=""save"" glob=""*.ext"">
        <working-directory>Build</working-directory>
        <command>$now = Get-Date ; Add-Content build.log ""Last build: $now""</command> 
    </action>
</goose>";

            var config = this.Parse(input);

            Assert.That(config.IsValid);
            Assert.That(config.Trigger, Is.EqualTo(Trigger.Save));
            Assert.That(config.Glob, Is.EqualTo("*.ext"));
            Assert.That(config.WorkingDirectory, Is.EqualTo("Build"));
            Assert.That(config.Command, Is.EqualTo(@"$now = Get-Date ; Add-Content build.log ""Last build: $now"""));
            Assert.That(config.ProjectRoot, Is.EqualTo("root"));
            Assert.That(config.Shell, Is.EqualTo(Shell.PowerShell));
        }

        private ActionConfiguration Parse(string input)
        {
            return this.parser.Parse("root", new MemoryStream(Encoding.UTF8.GetBytes(input)));
        }
    }
}
