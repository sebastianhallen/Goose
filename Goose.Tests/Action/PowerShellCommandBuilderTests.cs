namespace Goose.Tests.Action
{
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using NUnit.Framework;

    [TestFixture]
    public class PowerShellCommandBuilderTests
    {
        private PowerShellCommandBuilder commandBuilder;

        [SetUp]
        public void Before()
        {
            this.commandBuilder = new PowerShellCommandBuilder();
        }

        [Test]
        public void Should_set_working_directory()
        {            
            var command = this.commandBuilder.Build("working-dir", "", new CommandEvironmentVariables());

            Assert.That(command.WorkingDirectory, Is.EqualTo("working-dir"));
        }

        [Test]
        public void Should_set_command()
        {            
            var command = this.commandBuilder.Build("", "some command", new CommandEvironmentVariables());

            Assert.That(command.Command, Is.EqualTo("some command"));
        }

        [Test]
        public void Should_do_file_name_replacements_in_command()
        {
            var environmentVariables = new CommandEvironmentVariables { FilePath = "replace" };

            var command = this.commandBuilder.Build("", "some command with {file-path} substition", environmentVariables);

            Assert.That(command.Command, Is.EqualTo("some command with replace substition"));
        }

    }
}
