namespace Goose.Tests.Configuration
{
    using System.Linq;
    using Goose.Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigParser1Point0Tests
        : ConfigParserTests
    {        
        protected override string Version
        {
            get { return "1.0"; }
        }

        protected override ActionConfigurationParser Parser
        {
            get { return new ActionConfigurationParserVersion10(); }
        }

        [Test]
        public void Scope_should_be_per_project()
        {
            var input = this.CreateConfig(config => config.WithAction(_ => { }));

            var configuration = this.Parser.Parse("", input).Single();

            Assert.That(configuration.Scope, Is.EqualTo(CommandScope.Project));
        }
    }
}
