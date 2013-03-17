namespace Goose.Tests.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        
        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
        }

        [Test]
        public void Should_update_file_monitors_when_a_project_is_updated()
        {
            var project = new[] { "project.csproj" };
            A.CallTo(() => this.fileMonitor.IsMonitoredProject(project[0])).Returns(true);
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), A.Dummy<ActionConfiguration>());

            this.changeConsumer.ActOn(project, Trigger.Save);

            A.CallTo(() => this.fileMonitor.UnMonitor(A<string[]>._)).MustHaveHappened();
            A.CallTo(() => this.fileMonitor.MonitorProject(project[0], A<string>._)).MustHaveHappened();
        }

        [Test]
        public void Should_use_glob_from_configuration_used_when_initializing_when_refreshing_project_monitors()
        {
            var config = new ActionConfiguration("");
            A.CallTo(() => this.fileMonitor.IsMonitoredProject(A<string>._)).Returns(true);
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), config);

            this.changeConsumer.ActOn(new[] { "project.csproj" }, Trigger.Save);

            A.CallTo(() => this.fileMonitor.MonitorProject(A<string>._, config.Glob)).MustHaveHappened();
        }

        [Test]
        public void Should_queue_on_save_task_when_a_monitored_file_is_triggered_by_save()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredProject("project.csproj")).Returns(true);
            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>._)).Returns(new[] { A.Dummy<IGooseAction>() });
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), new ActionConfiguration(""));

            this.changeConsumer.ActOn(new[] { "project.csproj" }, Trigger.Save);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_only_use_project_files_to_create_actions_when_scope_is_per_project()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredProject("project.csproj")).Returns(true);
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("monitored0.file")).Returns(true);
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("monitored1.file")).Returns(true);
            var config = new ActionConfiguration(Trigger.Unknown, "", "", "", "", CommandScope.Project);
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), config);

            this.changeConsumer.ActOn(new[] { "project.csproj", "monitored0.file", "monitored1.file", "unmonitored.file" }, Trigger.Save);

            A.CallTo(() =>this.actionFactory.Create(config, A<IEnumerable<string>>.That.Matches(files => 
                files.SingleOrDefault() == "project.csproj"))).MustHaveHappened();
        }

        [Test]
        public void Should_only_use_non_project_files_to_create_actions_when_scope_is_per_file()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredProject("project.csproj")).Returns(true);
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("monitored0.file")).Returns(true);
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("monitored1.file")).Returns(true);
            var config = new ActionConfiguration(Trigger.Unknown, "", "", "", "", CommandScope.File);
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), config);

            this.changeConsumer.ActOn(new[] { "project.csproj", "monitored0.file", "monitored1.file", "unmonitored.file" }, Trigger.Save);

            A.CallTo(() => this.actionFactory.Create(config, A<IEnumerable<string>>.That.Matches(files =>
                files.SequenceEqual(new [] { "monitored0.file", "monitored1.file" })))).MustHaveHappened();
        }

        [Test]
        public void Should_queue_configured_action_when_a_monitored_non_project_file_is_deleted()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("file.less")).Returns(true);
            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>._)).Returns(new[] { A.Dummy<IGooseAction>() });
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), A.Dummy<ActionConfiguration>());

            this.changeConsumer.ActOn(new[] { "file.less" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_not_queue_action_when_a_project_file_is_deleted()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("project")).Returns(false);

            this.changeConsumer.ActOn(new[] { "project" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustNotHaveHappened();
        }

        [Test]
        public void Should_unmonitor_file_when_file_is_deleted()
        {
            var files = new[] { "file" };
            A.CallTo(() => this.fileMonitor.IsMonitoredFile(A<string>._)).Returns(true);
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), A.Dummy<ActionConfiguration>());

            this.changeConsumer.ActOn(files, Trigger.Delete);

            A.CallTo(() => this.fileMonitor.UnMonitor(A<IEnumerable<string>>.That.Matches(actual => actual.Single().Equals("file"))))
                .MustHaveHappened();
        }

        [Test]
        public void Should_not_have_to_update_a_project_file_to_trigger_project_scoped_actions()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("file")).Returns(true);
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("another file")).Returns(true);
            var config = new ActionConfiguration(Trigger.Unknown, "", "", "", "", CommandScope.File);
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), config);

            this.changeConsumer.ActOn(new [] {"file", "another file", "unmonitored files"}, Trigger.Save);

            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>.That.Matches(files =>
                files.SequenceEqual(new []{"file", "another file"})))).MustHaveHappened();
        }

        [Test]
        public void Should_queue_one_action_for_each_monitored_file_that_changed_when_using_file_scope()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("file")).Returns(true);
            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>._))
                .Returns(new[] {A.Dummy<IGooseAction>(), A.Dummy<IGooseAction>()});
            var config = new ActionConfiguration(Trigger.Unknown, "", "", "", "", CommandScope.File);
            this.changeConsumer.Initialize(A.Dummy<ISolutionProject>(), config);
            

            this.changeConsumer.ActOn(new[] { "file" }, Trigger.Save);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}
