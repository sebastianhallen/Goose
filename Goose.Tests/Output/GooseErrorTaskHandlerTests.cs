namespace Goose.Tests.Output
{
    using System;
    using System.Linq;
    using FakeItEasy;
    using Goose.Core.Action;
    using Goose.Core.Output;
    using NUnit.Framework;

    [TestFixture]
    public class GooseErrorTaskHandlerTests
    {
        [UnderTest] private GooseErrorTaskHandler errorTaskHandler;
        [Fake] private IErrorListProviderFacade errorListFacade;

        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
        }

        [Test]
        public void Should_save_added_tasks()
        {
            var command = new ShellCommand("", "");
            var tasks0 = new[] { this.CreateError(command), this.CreateError(command) };
            var tasks1 = new[] { this.CreateError(command), this.CreateError(command) };
            this.errorTaskHandler.Add(tasks0);
            this.errorTaskHandler.Add(tasks1);

            var existing = this.errorTaskHandler.Existing(command);

            Assert.That(existing, Is.EquivalentTo(tasks0.Concat(tasks1)));
        }

        [Test]
        public void Should_add_tasks_to_error_task_list()
        {
            this.errorTaskHandler.Add(new []{ A.Fake<IGooseErrorTask>() });

            A.CallTo(() => this.errorListFacade.Add(A<IGooseErrorTask>._)).MustHaveHappened();
        }

        [Test]
        public void Should_append_errors_to_error_task_list()
        {
            var error = this.CreateError(new ShellCommand("", ""));

            this.errorTaskHandler.Add(new[] { error });
            this.errorTaskHandler.Add(new[] { error });

            A.CallTo(() => this.errorListFacade.Add(A<IGooseErrorTask>._)).MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Test]
        public void Should_be_able_to_add_tasks_results_from_multiple_commands()
        {
            var command0 = new ShellCommand("a", "a");
            var error0 = this.CreateError(command0);
            var error1 = this.CreateError(command0);
            var tasks0 = new[] { error0, this.CreateError(new ShellCommand("", "")) };
            var tasks1 = new[] { this.CreateError(new ShellCommand("", "")), error1 };
            this.errorTaskHandler.Add(tasks0);
            this.errorTaskHandler.Add(tasks1);

            var existing = this.errorTaskHandler.Existing(command0);

            Assert.That(existing, Is.EquivalentTo(new [] { error0, error1 }));
        }

        [Test]
        public void Should_be_able_to_remove_tasks()
        {
            var command = new ShellCommand("", "");            
            var error0 = this.CreateError(command);            
            this.errorTaskHandler.Add(new [] { error0 });

            this.errorTaskHandler.Remove(command);

           Assert.That(this.errorTaskHandler.Existing(command).Any(), Is.False);
        }

        [Test]
        public void Should_remove_task_from_error_task_list()
        {
            var command = new ShellCommand(".", ".");
            var error = this.CreateError(command);
            this.errorTaskHandler.Add(new [] { error});

            this.errorTaskHandler.Remove(command);

            A.CallTo(() => this.errorListFacade.Remove(error)).MustHaveHappened();
        }

        private IGooseErrorTask CreateError(ShellCommand command)
        {
            var error = A.Fake<IGooseErrorTask>();
            A.CallTo(() => error.Command).Returns(command);

            return error;
        }

    }
}