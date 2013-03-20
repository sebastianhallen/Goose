namespace Goose.Tests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Goose.Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigParserTests
    {        
        protected ActionConfigurationParser Parser 
        {
            get { return new MultipleActionsConfigurationParser(); }
        }

        [Test]
        public void Should_create_invalid_command_configuration_when_an_empty_action_tag_is_encountered()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(_ => { }));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.IsValid, Is.False);
        }

        [TestCase(null)]
        [TestCase("     ")]
        public void Should_not_explode_when_unable_to_read_config(string input)
        {
            this.Parser.Parse("", input);
        }

        [Test]
        public void Should_not_explode_when_unable_to_read_config_stream()
        {
            Stream stream = null;

            this.Parser.Parse("", stream);
        }

        [Test]
        public void Should_set_working_directory_when_a_working_directory_tag_is_present()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithWorkingDirectory("Build")));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.RelativeWorkingDirectory, Is.EqualTo("Build"));
        }

        [Test]
        public void Action_config_with_only_build_directory_is_not_valid()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithWorkingDirectory("Build")));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.IsValid, Is.False);
        }

        [Test]
        public void Command_should_be_set_when_a_command_node_is_present()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithCommand("some command")));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.Command, Is.EqualTo("some command"));
        }

        [Test]
        public void action_config_with_only_command_should_not_be_valid()
        {
            var input = this.CreateConfig(config =>
                            config.WithAction(action =>
                                action.WithCommand("some command")));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.IsValid, Is.False);
        }

        [Test]
        public void Shell_should_be_powershell()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithCommand("some command")));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.Shell, Is.EqualTo(Shell.PowerShell));
        }

        [Test]
        public void Trigger_should_be_onsave_when_on_attribute_on_action_has_the_valud_save()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.TriggersOn("save")));

            var configuration = this.Parser.Parse("", input).Single();


            Assert.That(configuration.Trigger, Is.EqualTo(Trigger.Save));
        }

        [Test]
        public void Should_set_trigger_to_unknown_when_not_specified()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                { }));

            var configuration = this.Parser.Parse("", input).Single();


            Assert.That(configuration.Trigger, Is.EqualTo(Trigger.Unknown));
        }

        [Test]
        public void Should_set_trigger_to_unknown_when_unsupported_type_is_specified()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.TriggersOn("never")));

            var configuration = this.Parser.Parse("", input).Single();


            Assert.That(configuration.Trigger, Is.EqualTo(Trigger.Unknown));
        }

        [Test]
        public void Should_set_glob_to_default_value_when_no_glob_is_present()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.ForFilesMatching("*.ext")));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.Glob, Is.EqualTo("*.ext"));
        }

        [TestCase("file", CommandScope.File)]
        [TestCase("project", CommandScope.Project)]
        public void Should_be_able_to_parse_valid_command_scope(string configValue, CommandScope parsedValue)
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithScope(configValue)));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.Scope, Is.EqualTo(parsedValue));
        }

        [Test]
        public void Should_default_to_project_scope_when_unable_to_parse_scope()
        {
            var input = this.CreateConfig(config =>
                config.WithAction(action =>
                    action.WithScope("invalid scope")));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.Scope, Is.EqualTo(CommandScope.Project));
        }

        [Test]
        public void Should_default_scope_to_per_project_when_not_specified()
        {
            var input = this.CreateConfig(config => config.WithAction(_ => { }));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.Scope, Is.EqualTo(CommandScope.Project));
        }

        
        protected string CreateConfig(Action<GooseConfigRootBuilder> configure)
        {
            var configBuilder = new GooseTestConfigBuilder();
            var configurator = configBuilder.Version("ignored");

            configure(configurator);

            return configBuilder.Build();
        }

        protected class GooseTestConfigBuilder
        {
            private GooseConfigRootBuilder root;

            public string Build()
            {
                if (this.root == null)
                {
                    throw new InvalidOperationException("You need to configure the config first.");
                }

                return (this.root as IGooseConfigRootBuilderInternals).CreateSnapshot().ToString();
            }

            public GooseConfigRootBuilder Version(string version) 
            {
                this.root = new GooseConfigRootBuilder(version);
                return this.root;
            }   
        }

        internal interface IGooseConfigActionBuilderInternals
        {
            XElement Build();
        }

        public class GooseConfigActionBuilder
            : IGooseConfigActionBuilderInternals
        {
            private string trigger;
            private string glob;
            private string workingDirectory;
            private string command;
            private string scope;

            public GooseConfigActionBuilder TriggersOn(string trigger)
            {
                this.trigger = trigger;
                return this;
            }

            public GooseConfigActionBuilder ForFilesMatching(string glob)
            {
                this.glob = glob;
                return this;
            }

            public GooseConfigActionBuilder WithWorkingDirectory(string workingDirectory)
            {
                this.workingDirectory = workingDirectory;
                return this;
            }

            public GooseConfigActionBuilder WithCommand(string command)
            {
                this.command = command;
                return this;
            }

            public GooseConfigActionBuilder WithScope(string commandScope)
            {
                this.scope = commandScope;
                return this;
            }

            XElement IGooseConfigActionBuilderInternals.Build()
            {
                var actionXml = new XElement("action");
                if (this.trigger != null) actionXml.SetAttributeValue("on", this.trigger);
                if (this.glob != null) actionXml.SetAttributeValue("glob", this.glob);
                if (this.workingDirectory != null) actionXml.Add(new XElement("working-directory", this.workingDirectory));
                if (this.command != null) actionXml.Add(new XElement("command", this.command));
                if (this.scope != null) actionXml.Add(new XElement("scope", this.scope));
                return actionXml;
            }
        }

        internal interface IGooseConfigRootBuilderInternals
        {
            XElement CreateSnapshot();
        }

        public class GooseConfigRootBuilder
            : IGooseConfigRootBuilderInternals
        {
            private readonly string version;
            private readonly IList<XElement> actions;
            public GooseConfigRootBuilder(string version)
            {
                this.version = version;
                this.actions = new List<XElement>();
            }

            public GooseConfigRootBuilder WithAction(Action<GooseConfigActionBuilder> configuration)
            {
                var action = new GooseConfigActionBuilder();
                configuration(action);
                this.actions.Add((action as IGooseConfigActionBuilderInternals).Build());

                return this;
            }

            XElement IGooseConfigRootBuilderInternals.CreateSnapshot()
            {
                var root = new XElement("goose", new XAttribute("version", this.version));
                foreach (var action in this.actions)
                {
                    root.Add(action);
                }

                return root;
            }
        }
    }
}
