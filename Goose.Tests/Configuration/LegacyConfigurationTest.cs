namespace Goose.Tests.Configuration
{
    using Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class LegacyConfigurationTest
    {
        private LegacyConfigurationParser parser;

        [SetUp]
        public void Before()
        {
            this.parser = new LegacyConfigurationParser();
        }

        [Test]
        public void Should_be_able_to_parse_a_complete_legacy_configuration()
        {
            var input = @"
<compile-less>
  <build-directory>Build</build-directory>
  <compile-command>$now = Get-Date ; Add-Content build.log ""Last build: $now""</compile-command> 
</compile-less>";

            var config = this.parser.Parse("", input);

            Assert.That(config.IsValid);
        }
    }
}
