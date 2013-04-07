namespace Goose.Tests.Action
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using Goose.Core.Output;
    using NUnit.Framework;

    [TestFixture]
    public class PowerShellTaskFactoryIntegrationTests
    {
        [UnderTest]private PowerShellTaskFactory taskFactory;
        [Fake] private IOutputService outputService;
        private InputSavingCommandLogParser commandLogParser;

        [SetUp]
        public void Before()
        {
            this.outputService = A.Fake<IOutputService>();
            this.commandLogParser = new InputSavingCommandLogParser();
            this.taskFactory = new PowerShellTaskFactory(this.outputService, this.commandLogParser);
        }

        [Test]
        public void Should_be_able_to_invoke_a_simple_command()
        {
            var command = this.CreateCommand(@"ls");

            var task = this.taskFactory.Create(command);
            task.RunSynchronously();

            var output = this.commandLogParser.Input;
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

        private class InputSavingCommandLogParser
            : ICommandLogParser
        {
            public string Input;

            public CommandOutput Parse(string buildLog)
            {
                this.Input = buildLog;
                return A.Dummy<CommandOutput>();
            }
        }
    }
}
