namespace Goose.Tests.Configuration
{
    using System.Linq;
    using Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class LegacyFallbackActionConfigurationParserTests
    {
        private LegacyFallbackActionConfigurationParser parser;

        [SetUp]
        public void Before()
        {
            this.parser = new LegacyFallbackActionConfigurationParser();
        }

        [Test]
        public void Should_be_able_to_parse_a_complete_legacy_configuration()
        {
            var input = @"
<compile-less>
  <build-directory>Build</build-directory>
  <compile-command>$now = Get-Date ; Add-Content build.log ""Last build: $now""</compile-command> 
</compile-less>";

            var config = this.parser.Parse("", input).Single();

            Assert.That(config.IsValid);
        }

        [Test]
        public void Should_be_able_to_parse_a_complete_action_configuration()
        {
            var input = @"
<goose version=""1.0"">
    <action on=""save"" glob=""glob"">
        <working-directory>Build</working-directory>
        <command>$now = Get-Date ; Add-Content build.log ""Last build: $now""</command> 
    </action>
</goose>";

            var config = this.parser.Parse("", input).Single();

            Assert.That(config.IsValid);
        }

        [Test]
        public void Should_favor_result_from_action_config_over_legacy_config_when_parsing_uncomplete_config()
        {
            var input = @"
<goose version=""1.0"">
    <action on=""save"">
        <command>command</command> 
    </action>
</goose>";

            var config = this.parser.Parse("", input).Single();

            Assert.That(config.Command, Is.EqualTo("command"));
        }
    }
}
