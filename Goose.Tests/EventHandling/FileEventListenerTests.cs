namespace Goose.Tests.EventHandling
{
    using Core.Action;
    using Core.Dispatcher;
    using FakeItEasy;
    using Goose.Core.Configuration;
    using Goose.Core.Solution;
    using Goose.Core.Solution.EventHandling;
    using Microsoft.VisualStudio.Shell.Interop;
    using NUnit.Framework;

    [TestFixture]
    public class FileEventListenerTests
    {
        [UnderTest] private FileEventListener eventListener;
        [Fake] private ISolutionFilesService solutionFilesService;
        [Fake] private IFileMonitor fileMonitor;
        [Fake] private IGooseActionFactory actionFactory;
        [Fake] private IOnChangeTaskDispatcher taskDispatcher;
        [Fake] private IFileChangeSubscriber fileChangeSubscriber;        
        
        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
        }

        [Test]
        public void Should_not_monitor_project_when_project_path_is_empty()
        {
            var project = A.Fake<ISolutionProject>();
            A.CallTo(() => project.ProjectFilePath).Returns(null);

            this.eventListener.Initialize(project, A.Dummy<ActionConfiguration>());

            A.CallTo(() => this.fileMonitor.MonitorProject(A<string>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void File_change_subscriber_should_be_attached_to_monitor()
        {
            A.CallTo(() => this.fileChangeSubscriber.Attach(this.eventListener)).MustHaveHappened();
        }

        [Test]
        public void Should_unmonitor_file_when_file_is_deleted()
        {
            var files = new[] {"file"};

            this.eventListener.ActOn(files, Trigger.Delete);

            A.CallTo(() => this.fileMonitor.UnMonitor(files)).MustHaveHappened();
        }

        [Test]
        public void Should_update_file_monitors_when_a_project_is_updated()
        {
            var project = new[] {"project.csproj"};
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
            
            this.eventListener.ActOn(new [] {"project.csproj"}, Trigger.Save);

            A.CallTo(() => this.fileMonitor.MonitorProject(A<string>._, config.Glob)).MustHaveHappened();
        }

        [Test]
        public void Should_not_monitor_files_when_configuration_is_not_valid()
        {
            this.eventListener.Initialize(A.Dummy<ISolutionProject>(), new ActionConfiguration(""));

            A.CallTo(() => this.solutionFilesService.Projects).MustNotHaveHappened();
        }

        [Test]
        public void Should_queue_on_save_task_when_a_monitored_file_is_triggered_by_save()
        {
            this.eventListener.ActOn(new [] { "project.csproj"}, Trigger.Save);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_queue_configured_action_when_a_monitored_non_project_file_is_deleted()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("file.less")).Returns(true);

            this.eventListener.ActOn(new[] { "file.less" }, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_not_queue_action_when_a_project_file_is_deleted()
        {
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("project")).Returns(false);

            this.eventListener.ActOn(new string[] { "project"}, Trigger.Delete);

            A.CallTo(() => this.taskDispatcher.QueueOnChangeTask(A<IGooseAction>._)).MustNotHaveHappened();
        }

        [Test]
        public void Should_trigger_deletion_action_when_a_watched_file_is_deleted()
        {
            var file = new[] { "file" };

            this.eventListener.FilesChanged(1, file, new [] { (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del });

            A.CallTo(() => this.fileMonitor.UnMonitor(file)).MustHaveHappened();
        }

        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Add)]
        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Size)]
        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Time)]
        public void Should_trigger_save_action_when_a_watched_file_is_saved(_VSFILECHANGEFLAGS changeFlag)
        {
            var file = new[] {"file"};

            this.eventListener.FilesChanged(1, file, new [] { (uint)changeFlag });

            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._)).MustHaveHappened();
        }

        [Test]
        public void Should_dispose_file_monitor_when_disposing()
        {
            this.eventListener.Dispose();

            A.CallTo(() => this.fileMonitor.Dispose()).MustHaveHappened();
        }
    }
}
