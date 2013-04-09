namespace Goose.Tests.Output
{
    using System.Linq;
    using FakeItEasy;
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using Goose.Core.Output;
    using NUnit.Framework;

    [TestFixture]
    public class CommandErrorReporterTests
    {
        [UnderTest] private CommandErrorReporter errorReporter;
        [Fake] private IErrorTaskHandler errorTaskHandler;
        [Fake] private ICommandLogParser logParser;

        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
        }

        [Test]
        public void Should_send_error_task_to_error_task_handler_when_reporting_error()
        {
            var commandOutput = this.BuildCommandOutput("some error");
            A.CallTo(() => this.logParser.Parse(A<string>._)).Returns(commandOutput);

            this.errorReporter.Report(A.Fake<ShellCommand>(), A.Fake<CommandResult>());

            A.CallTo(() => this.errorTaskHandler.Add(A<GooseErrorTask>._)).MustHaveHappened();
        }

        [Test]
        public void Should_parse_command_result_when_creating_error_task()
        {
            var result = new CommandResult("build log", null, null);
            
            this.errorReporter.Report(A.Fake<ShellCommand>(), result);

            A.CallTo(() => this.logParser.Parse("build log")).MustHaveHappened();
        }

        private CommandOutput BuildCommandOutput(params string[] messages)
        {
            var errors = messages.Select(message => new CommandOutputItem
                {
                    Type = CommandOutputItemType.Error,
                    Message = message
                });
            return new CommandOutput
                {
                    Results = errors.ToList()
                };
        }
    }
}
