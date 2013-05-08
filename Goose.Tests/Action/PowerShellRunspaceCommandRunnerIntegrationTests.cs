namespace Goose.Tests.Action
{
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using NUnit.Framework;
    using System.IO;
    using System.Reflection;

    [TestFixture]
    public class PowerShellRunspaceCommandRunnerIntegrationTests
    {        
        private IShellCommandRunner commandRunner;

        [SetUp]
        public void Before()
        {
            this.commandRunner = new PowerShellCommandRunner();
        }

        [Test]
        public void Should_be_able_to_run_simple_command()
        {
            var command = this.CreateCommand("ls");

            var output = this.commandRunner.RunCommand(command);

            Assert.That(output.Result.Contains("Goose.dll"));
        }

        [Test]
        public void Should_be_able_to_run_commands_that_require_a_host()
        {
            var command = this.CreateCommand(@"write-host ""some output""");

            var output = this.commandRunner.RunCommand(command);

            Assert.That(output.Output, Is.EqualTo("some output"));
        }

        private ShellCommand CreateCommand(string command)
        {
            return new ShellCommand(AssemblyDirectory(), command);
        }

        public static string AssemblyDirectory()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(
                assembly.GetName().CodeBase
                .Replace("file:///", ""));
        }
    }
}
