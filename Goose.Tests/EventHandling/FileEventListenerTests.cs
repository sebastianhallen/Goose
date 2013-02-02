//namespace Goose.Tests.EventListener
//{
//    using Core.Configuration;
//    using Core.EventListener;
//    using Core.Solution;
//    using FakeItEasy;
//    using NUnit.Framework;

//    [TestFixture]
//    public class FileEventListenerTests
//    {
//        [UnderTest] private FileEventListener eventListener;
//        [Fake] private ISolutionFilesService solutionFilesService;
//        [Fake] private IFileMonitor fileMonitor;
//        [Fake] private IGlobMatcher globMatcher;

//        [SetUp]
//        public void Before()
//        {
//            Fake.InitializeFixture(this);

//            A.CallTo(() => this.globMatcher.Matches(A<string>._, A<string>._)).Returns(true);
//        }

//        [Test]
//        public void Should_add_all_files_with_matching_file_name_to_monitored_files_when_initializing()
//        {
//            var project = A.Fake<ISolutionProject>();
//            A.CallTo(() => project.Files).Returns(new[] {A.Dummy<FileInProject>(), A.Dummy<FileInProject>()});
//            A.CallTo(() => this.solutionFilesService.Projects).Returns(new[] { project });

//            this.eventListener.Initialize(A.Dummy<ActionConfiguration>());

//            A.CallTo(() => this.fileMonitor.MonitorFile(A<FileInProject>._, A<Trigger>._)).MustHaveHappened(Repeated.Exactly.Twice);
//        }

//        [Test]
//        public void Should_add_all_project_files_to_monitored_project_when_initializing()
//        {
//            var projects = new[] {A.Dummy<ISolutionProject>(), A.Dummy<ISolutionProject>()};
//            A.CallTo(() => this.solutionFilesService.Projects).Returns(projects);

//            this.eventListener.Initialize(A.Dummy<ActionConfiguration>());

//            A.CallTo(() => this.fileMonitor.MonitorProject(A<string>._, A<ActionConfiguration>._)).MustHaveHappened(Repeated.Exactly.Twice);
//        }

//        [Test]
//        public void Should_update_monitored_files_in_project_when_project_file_is_changed()
//        {
//            var project = A.Fake<ISolutionProject>();
//            A.CallTo(() => this.fileMonitor.IsMonitoredProject("project.csproj")).Returns(true);            
//            A.CallTo(() => project.Files).Returns(new[] { A.Dummy<FileInProject>() });
            
//            this.eventListener.FilesChanged(0, new[] { "project.csproj" }, new[] { (uint)2 });

//            A.CallTo(() => this.fileMonitor.UpdateFileMonitorsForProject(A<string>._)).MustHaveHappened();
//        }
//    }
//}
