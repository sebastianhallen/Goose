namespace Goose.Tests.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using FakeItEasy;
    using Goose.Core.Action;
    using Goose.Core.Configuration;
    using Goose.Core.Dispatcher;
    using Goose.Core.Solution;
    using Goose.Core.Solution.EventHandling;
    using NUnit.Framework;

    [TestFixture]
    public class FileChangeConsumerTests
    {
        [UnderTest] private FileEventListener changeConsumer;        
        [Fake] private IFileMonitor fileMonitor;
        [Fake] private IGooseActionFactory actionFactory;
        [Fake] private IOnChangeTaskDispatcher taskDispatcher;

        private TestSubscriptionBuilder subscriptionBuilder;
        
        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
            this.subscriptionBuilder = new TestSubscriptionBuilder(this.fileMonitor, this.changeConsumer, this.actionFactory);
        }

        private class TestSubscriptionBuilder
        {
            private readonly IFileMonitor fileMonitor;
            private readonly FileEventListener changeConsumer;
            private readonly IGooseActionFactory actionFactory;

            public TestSubscriptionBuilder(IFileMonitor fileMonitor, FileEventListener changeConsumer, IGooseActionFactory actionFactory)
            {
                this.fileMonitor = fileMonitor;
                this.changeConsumer = changeConsumer;
                this.actionFactory = actionFactory;

                A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>._))
                    .WithAnyArguments()
                    .ReturnsLazily(call => ((IEnumerable<string>)call.Arguments[1]).Select(_ => A.Fake<IGooseAction>()));
            }

            public TestSubscriptionBuilder MonitorFile(params string[] files)
            {
                foreach (var file in files)
                {
                    A.CallTo(() => this.fileMonitor.IsMonitoredFile(file)).Returns(true);
                }
                return this;
            }

            public TestSubscriptionBuilder MonitorProject(params string[] projecs)
            {
                foreach (var project in projecs)
                {
                    A.CallTo(() => this.fileMonitor.IsMonitoredProject(project)).Returns(true);
                }

                return this;
            }

            public TestSubscriptionBuilder MonitorAnyProject()
            {
                A.CallTo(() => this.fileMonitor.IsMonitoredProject(A<string>._)).Returns(true);
                return this;
            }

            public void WithConfiguration(ActionConfiguration configuration)
            {
                this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), configuration);
            }

            public void WithAnyConfiguration()
            {
                this.WithConfiguration(A.Dummy<ActionConfiguration>());
            }

            public void WithPerFileConfiguration(string glob = "")
            {
                this.WithConfiguration(new ActionConfiguration(Trigger.Save, glob, "", "", "", CommandScope.File));
            }

            public void WithPerProjectScopedConfiguration(string glob = "")
            {
                this.WithConfiguration(new ActionConfiguration(Trigger.Save, glob, "", "", "", CommandScope.Project));
            }
        }

        [Test]
        public void Should_update_file_monitors_when_a_project_is_updated()
        {
            this.subscriptionBuilder
                .MonitorProject("project.csproj")
                .WithAnyConfiguration();

            this.changeConsumer.ActOn(new [] { "project.csproj" }, Trigger.Save);

            A.CallTo(() => this.fileMonitor.UnMonitor(A<string[]>._)).MustHaveHappened();
            A.CallTo(() => this.fileMonitor.MonitorProject("project.csproj", A<string>._)).MustHaveHappened();
        }

        [Test]
        public void Should_use_glob_from_configuration_used_when_initializing_when_refreshing_project_monitors()
        {
            this.subscriptionBuilder
                .MonitorAnyProject()
                .WithPerProjectScopedConfiguration(glob: "glob");
            
            this.changeConsumer.ActOn(new[] { "project.csproj" }, Trigger.Save);

            A.CallTo(() => this.fileMonitor.MonitorProject(A<string>._, "glob")).MustHaveHappened();
        }

        [Test]
        public void Should_queue_on_change_task_when_a_per_project_scope_monitored_project_file_is_triggered_by_save()
        {
            this.subscriptionBuilder
                .MonitorProject("project.csproj")
                .WithPerProjectScopedConfiguration();

            this.changeConsumer.ActOn(new[] { "project.csproj" }, Trigger.Save);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_create_actions_for_all_changed_non_project_files_when_scope_is_per_file()
        {
            this.subscriptionBuilder
                .MonitorProject("project.csproj")
                .MonitorFile("monitored0.file", "monitored1.file")
                .WithPerFileConfiguration();
            
            this.changeConsumer.ActOn(new[] { "project.csproj", "monitored0.file", "monitored1.file", "unmonitored.file" }, Trigger.Save);

            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>.That.Matches(files =>
                files.SequenceEqual(new[] { "monitored0.file", "monitored1.file" })))).MustHaveHappened();
        }

        [Test]
        public void Should_queue_task_when_a_monitored_non_project_file_is_deleted_with_per_project_scope()
        {
            this.subscriptionBuilder
                .MonitorFile("file.less")
                .WithPerProjectScopedConfiguration();

            this.changeConsumer.ActOn(new[] { "file.less" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_not_queue_task_when_a_monitored_non_project_file_is_deleted_with_per_file_scope()
        {
            this.subscriptionBuilder
                .MonitorFile("file.less")
                .WithPerFileConfiguration();

            this.changeConsumer.ActOn(new[] { "file.less" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustNotHaveHappened();
        }

        [Test]
        public void Should_not_queue_action_when_a_project_file_is_deleted_with_per_project_scope()
        {
            this.subscriptionBuilder
                .MonitorProject("project.csproj")
                .WithPerProjectScopedConfiguration();
         
            this.changeConsumer.ActOn(new[] { "project.csproj" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustNotHaveHappened();
        }

        [Test]
        public void Should_not_queue_task_when_a_monitored_project_file_is_deleted_with_per_file_scope()
        {
            this.subscriptionBuilder
                .MonitorProject("project.csproj")
                .WithPerFileConfiguration();

            this.changeConsumer.ActOn(new[] { "project.csproj" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustNotHaveHappened();
        }

        
        [Test]
        public void Should_unmonitor_file_when_file_is_deleted()
        {
            this.subscriptionBuilder
                .MonitorFile("file")
                .WithAnyConfiguration();

            this.changeConsumer.ActOn(new[] { "file" }, Trigger.Delete);

            A.CallTo(() => this.fileMonitor.UnMonitor(A<IEnumerable<string>>.
                That.Matches(actual => actual.Single().Equals("file"))))
                .MustHaveHappened();
        }

        [Test]
        public void Should_queue_one_action_for_each_monitored_file_that_changed_when_using_file_scope()
        {
            this.subscriptionBuilder
                .MonitorFile("file", "another file")
                .WithPerFileConfiguration();
            
            this.changeConsumer.ActOn(new[] { "file", "another file" }, Trigger.Save);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}
