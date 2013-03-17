namespace Goose.Tests.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        public void Should_not_monitor_files_when_configuration_is_not_valid()
        {
            this.eventListener.Initialize(A.Dummy<ISolutionProject>(), new ActionConfiguration(""));

            A.CallTo(() => this.solutionFilesService.Projects).MustNotHaveHappened();
        }
       
        [Test]
        public void Should_trigger_deletion_action_when_a_watched_file_is_deleted()
        {
            var file = new[] { "file" };
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("file")).Returns(true);
            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>._)).Returns(new[] { A.Dummy<IGooseAction>() });

            this.eventListener.FilesChanged(1, file, new [] { (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del });

            A.CallTo(() => this.fileMonitor.UnMonitor(A<IEnumerable<string>>.That.Matches(actual => actual.Single().Equals("file")))).MustHaveHappened();
        }

        [Test]
        public void Should_not_unmonitor_file_when_an_unwatched_file_is_deleted()
        {
            var file = new[] { "file" };

            this.eventListener.FilesChanged(1, file, new[] { (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del });

            A.CallTo(() => this.fileMonitor.UnMonitor(A<IEnumerable<string>>._)).MustNotHaveHappened();
        }

        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Add)]
        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Size)]
        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Time)]
        public void Should_trigger_save_action_when_a_watched_file_is_saved(_VSFILECHANGEFLAGS changeFlag)
        {
            var file = new[] {"file"};
            A.CallTo(() => this.fileMonitor.IsMonitoredFile("file")).Returns(true);
            
            this.eventListener.FilesChanged(1, file, new [] { (uint)changeFlag });

            A.CallTo(() => this.actionFactory.Create(A<ActionConfiguration>._, A<IEnumerable<string>>._)).MustHaveHappened();
        }

        [Test]
        public void Should_dispose_file_monitor_when_disposing()
        {
            this.eventListener.Dispose();

            A.CallTo(() => this.fileMonitor.Dispose()).MustHaveHappened();
        }
    }
}
