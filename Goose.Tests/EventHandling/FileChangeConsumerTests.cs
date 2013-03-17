namespace Goose.Tests.EventHandling
{
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
        [UnderTest] private FileEventListener eventListener;        
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
            this.eventListener.Initialize(A.Dummy<ISolutionProject>(), A.Dummy<ActionConfiguration>());

            this.eventListener.ActOn(project, Trigger.Save);

            A.CallTo(() => this.fileMonitor.UnMonitor(A<string[]>._)).MustHaveHappened();
            A.CallTo(() => this.fileMonitor.MonitorProject(project[0], A<string>._)).MustHaveHappened();
        }

        [Test]
        public void Should_use_glob_from_configuration_used_when_initializing_when_refreshing_project_monitors()
        {
            var config = new ActionConfiguration("");
            A.CallTo(() => this.fileMonitor.IsMonitoredProject(A<string>._)).Returns(true);
            this.eventListener.Initialize(A.Dummy<ISolutionProject>(), config);

            this.eventListener.ActOn(new[] { "project.csproj" }, Trigger.Save);

            A.CallTo(() => this.fileMonitor.MonitorProject(A<string>._, config.Glob)).MustHaveHappened();
        }

        [Test]
        public void Should_queue_on_save_task_when_a_monitored_file_is_triggered_by_save()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredProject("project.csproj")).Returns(true);
            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>._)).Returns(new[] { A.Dummy<IGooseAction>() });
            this.eventListener.Initialize(A.Dummy<ISolutionProject>(), new ActionConfiguration(""));

            this.eventListener.ActOn(new[] { "project.csproj" }, Trigger.Save);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_queue_configured_action_when_a_monitored_non_project_file_is_deleted()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("file.less")).Returns(true);
            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>._))
             .Returns(new[] { A.Dummy<IGooseAction>() });

            this.eventListener.ActOn(new[] { "file.less" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_not_queue_action_when_a_project_file_is_deleted()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("project")).Returns(false);

            this.eventListener.ActOn(new[] { "project" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustNotHaveHappened();
        }

        [Test]
        public void Should_unmonitor_file_when_file_is_deleted()
        {
            var files = new[] { "file" };
            A.CallTo(() => this.fileMonitor.IsMonitoredFile(A<string>._)).Returns(true);

            this.eventListener.ActOn(files, Trigger.Delete);

            A.CallTo(() => this.fileMonitor.UnMonitor(A<IEnumerable<string>>.That.Matches(actual => actual.Single().Equals("file"))))
                .MustHaveHappened();
        }
    }
}
