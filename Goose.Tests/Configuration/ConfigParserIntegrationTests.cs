namespace Goose.Tests.Configuration
{
    using System.Linq;
    using Goose.Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigParserIntegrationTests
    {
        [Test]
        public void Should_be_able_to_parse_a_complete_1_point_0_action_configuration()
        {
            var input = @"
        <goose version=""1.0"">
            <action on=""save"" glob=""*.ext"">
                <working-directory>BuildLess</working-directory>
                <command>$now = Get-Date ; Add-Content build.log ""Last ext build: $now""</command> 
            </action>
            <action on=""save"" glob=""*.css"">
                <working-directory>MinifyCss</working-directory>
                <command>$now = Get-Date ; Add-Content build.log ""Last css build: $now""</command> 
            </action>
        </goose>";

            var parser = new ActionConfigurationParser();
            var configs = parser.Parse("", input);

            var config = configs.First();
            Assert.That(config.IsValid);
            Assert.That(config.Trigger, Is.EqualTo(Trigger.Save));
            Assert.That(config.Glob, Is.EqualTo("*.ext"));
            Assert.That(config.WorkingDirectory, Is.EqualTo("BuildLess"));
            Assert.That(config.Command, Is.EqualTo(@"$now = Get-Date ; Add-Content build.log ""Last ext build: $now"""));
            Assert.That(config.ProjectRoot, Is.EqualTo("root"));
            Assert.That(config.Shell, Is.EqualTo(Shell.PowerShell));

            config = configs.Last();
            Assert.That(config.IsValid);
            Assert.That(config.Trigger, Is.EqualTo(Trigger.Save));
            Assert.That(config.Glob, Is.EqualTo("*.css"));
            Assert.That(config.WorkingDirectory, Is.EqualTo("MinifyCss"));
            Assert.That(config.Command, Is.EqualTo(@"$now = Get-Date ; Add-Content build.log ""Last css build: $now"""));
            Assert.That(config.ProjectRoot, Is.EqualTo("root"));
            Assert.That(config.Shell, Is.EqualTo(Shell.PowerShell));
        }
    }
}
