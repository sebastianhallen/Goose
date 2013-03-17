namespace Goose.Tests.Action
{
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using NUnit.Framework;

    [TestFixture]
    public class PowerShellCommandBuilderTests
    {
        [Test]
        public void Should_set_working_directory()
        {
            var commandBuilder = new PowerShellCommandBuilder();

            var command = commandBuilder.Build("working-dir", "", new CommandEvironmentVariables());

            Assert.That(command.WorkingDirectory, Is.EqualTo("working-dir"));
        }

        [Test]
        public void Should_set_command()
        {
            var commandBuilder = new PowerShellCommandBuilder();

            var command = commandBuilder.Build("", "some command", new CommandEvironmentVariables());

            Assert.That(command.Command, Is.EqualTo("some command"));
        }

    }
}
