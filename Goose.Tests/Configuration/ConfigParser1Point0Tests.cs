namespace Goose.Tests.Configuration
{
    using System.IO;
    using System.Linq;
    using Goose.Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigParser1Point0Tests
        : ConfigParserTests
    {
        private ActionConfigurationParser parser;

        protected override string Version
        {
            get { return "1.0"; }
        }

        [SetUp]
        public void BeforeTest()
        {
            this.parser = new ActionConfigurationParser();
        }

        [Test]
        public void Should_create_invalid_command_configuration_when_an_empty_action_tag_is_encountered()
        {
            var input = this.CreateConfig(config => 
                config.WithAction(_ => {}));

            var configuration = this.parser.Parse("", input).Single();

            Assert.That(configuration.IsValid, Is.False);
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
            var input = this.CreateConfig(config => 
                config.WithAction(action =>
                    action.WithWorkingDirectory("Build")));

            var configuration = this.parser.Parse("", input).Single();

            Assert.That(configuration.WorkingDirectory, Is.EqualTo("Build"));
        }

        [Test]
        public void Action_config_with_only_build_directory_is_not_valid()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithWorkingDirectory("Build")));

            var configuration = this.parser.Parse("", input).Single();

            Assert.That(configuration.IsValid, Is.False);
        }

        [Test]
        public void Command_should_be_set_when_a_command_node_is_present()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithCommand("some command")));

            var configuration = this.parser.Parse("", input).Single();

            Assert.That(configuration.Command, Is.EqualTo("some command"));
        }

        [Test]
        public void action_config_with_only_command_should_not_be_valid()
        {
            var input = this.CreateConfig(config =>
                            config.WithAction(action =>
                                action.WithCommand("some command")));

            var configuration = this.parser.Parse("", input).Single();

            Assert.That(configuration.IsValid, Is.False);
        }

        [Test]
        public void Shell_should_be_powershell()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithCommand("some command")));

            var configuration = this.parser.Parse("", input).Single();

            Assert.That(configuration.Shell, Is.EqualTo(Shell.PowerShell));
        }

        [Test]
        public void Trigger_should_be_onsave_when_on_attribute_on_action_has_the_valud_save()
        {            
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.TriggersOn("save")));

            var configuration = this.parser.Parse("", input).Single();


            Assert.That(configuration.Trigger, Is.EqualTo(Trigger.Save));
        }

        [Test]
        public void Should_set_trigger_to_unknown_when_not_specified()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    { }));

            var configuration = this.parser.Parse("", input).Single();


            Assert.That(configuration.Trigger, Is.EqualTo(Trigger.Unknown));
        }

        [Test]
        public void Should_set_trigger_to_unknown_when_unsupported_type_is_specified()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.TriggersOn("never")));

            var configuration = this.parser.Parse("", input).Single();


            Assert.That(configuration.Trigger, Is.EqualTo(Trigger.Unknown));
        }

        [Test]
        public void Should_set_glob_to_default_value_when_no_glob_is_present()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.ForFilesMatching("*.ext")));

            var configuration = this.parser.Parse("", input).Single();

            Assert.That(configuration.Glob, Is.EqualTo("*.ext"));
        }
    }
}
