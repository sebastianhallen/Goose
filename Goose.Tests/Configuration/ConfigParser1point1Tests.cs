namespace Goose.Tests.Configuration
{
    using System.Linq;
    using Goose.Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigParser1point1Tests
        : ConfigParserTests
    {
        protected override string Version
        {
            get { return "1.1"; }
        }

        protected override ActionConfigurationParser Parser
        {
            get { return new ActionConfigurationParserVersion11(); }
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
    }
}
