namespace Goose.Tests.Action
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using NUnit.Framework;

    [TestFixture, Explicit]
    public class PowerShellRunspaceCommandRunnerIntegrationTests
        : ShellCommandRunnerIntegrationTests
    {
        protected override IShellCommandRunner CreateCommandRunner()
        {
            return new PowerShellRunspaceCommandRunner();
        }
    }

    [TestFixture, Ignore]
    public class PowerShellPSCommandRunnerIntegrationTests
        : ShellCommandRunnerIntegrationTests
    {
        protected override IShellCommandRunner CreateCommandRunner()
        {
            return new PowerShellPSCommandRunner();
        }
    }

    [TestFixture]
    public abstract class ShellCommandRunnerIntegrationTests
    {
        protected abstract IShellCommandRunner CreateCommandRunner();
        private IShellCommandRunner commandRunner;

        [SetUp]
        public void Before()
        {
            this.commandRunner = this.CreateCommandRunner();
        }

        [Test]
        public void Should_be_able_to_run_simple_command()
        {
            var command = this.CreateCommand("ls");

            var output = this.commandRunner.RunCommand(command);

            Assert.That(string.IsNullOrWhiteSpace(output), Is.False);
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
