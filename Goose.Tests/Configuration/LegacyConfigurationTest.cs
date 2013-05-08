namespace Goose.Tests.Configuration
{
    using System.Linq;
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

            var config = this.parser.Parse("solution", "root", input).Single();

            Assert.That(config.IsValid);
            Assert.That(config.Command, Is.EqualTo(@"$now = Get-Date ; Add-Content build.log ""Last build: $now"""));
            Assert.That(config.Glob, Is.EqualTo("*.less"));
            Assert.That(config.SolutionRoot, Is.EqualTo("solution"));
            Assert.That(config.ProjectRoot, Is.EqualTo("root"));
            Assert.That(config.Shell, Is.EqualTo(Shell.PowerShell));
            Assert.That(config.Trigger, Is.EqualTo(Trigger.Save));
            Assert.That(config.Scope, Is.EqualTo(CommandScope.Project));
            Assert.That(config.RelativeWorkingDirectory, Is.EqualTo("Build"));
        }
    }
}
