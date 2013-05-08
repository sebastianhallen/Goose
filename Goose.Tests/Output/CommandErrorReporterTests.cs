namespace Goose.Tests.Output
{
    using System.Collections.Generic;
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
            this.ExpectCommandOutput("some error");
            
            this.errorReporter.Report(A.Fake<ShellCommand>(), A.Fake<CommandResult>());

            A.CallTo(() => this.errorTaskHandler.Add(A<IEnumerable<IGooseErrorTask>>._)).MustHaveHappened();
        }

        [Test]
        public void Should_parse_command_result_when_creating_error_task()
        {
            var result = new CommandResult("build log", null, null);
            
            this.errorReporter.Report(A.Fake<ShellCommand>(), result);

            A.CallTo(() => this.logParser.Parse("build log")).MustHaveHappened();
        }

        [Test]
        public void Should_remove_previous_errors_from_matching_command_when_handling_new_errors()
        {
            var task = A.Fake<IGooseErrorTask>();
            var command = new ShellCommand("", "");
            A.CallTo(() => task.Command).Returns(command);
            A.CallTo(() => this.errorTaskHandler.Existing(command)).Returns(new[] { task });

            this.errorReporter.Report(command, A.Fake<CommandResult>());

            A.CallTo(() => this.errorTaskHandler.Remove(command)).MustHaveHappened();
        }

        private void ExpectCommandOutput(params string[] messages)
        {
            var errors = messages.Select(message => new CommandOutputItem
                {
                    Type = CommandOutputItemType.Error,
                    Message = message
                });
            var output = new CommandOutput
                {
                    Results = errors.ToList()
                };
            A.CallTo(() => this.logParser.Parse(A<string>._)).Returns(output);

        }
    }
}
