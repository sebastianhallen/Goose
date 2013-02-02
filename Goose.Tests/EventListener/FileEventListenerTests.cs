namespace Goose.Tests.EventListener
{
    using Core.EventListener;
    using Core.Solution;
    using FakeItEasy;
    using NUnit.Framework;

    [TestFixture]
    public class FileEventListenerTests
    {
        [UnderTest] private FileEventListener eventListener;
        [Fake] private ISolutionFilesService solutionFilesService;
        [Fake] private IFileMonitor fileMonitor;
        [Fake] private IGlobMatcher globMatcher;

        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);

            A.CallTo(() => this.globMatcher.Matches(A<string>._, A<string>._)).Returns(true);
        }

        [Test]
        public void Should_add_all_files_with_matching_file_name_to_monitored_files_when_initializing()
        {
            var project = A.Fake<ISolutionProject>();
            A.CallTo(() => project.Files).Returns(new[] {A.Dummy<FileInProject>(), A.Dummy<FileInProject>()});
            A.CallTo(() => this.solutionFilesService.Projects).Returns(new[] { project });

            this.eventListener.Initialize("");

            A.CallTo(() => this.fileMonitor.MonitorFile(A<FileInProject>._)).MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Test]
        public void Should_add_all_project_files_to_monitored_project_when_initializing()
        {
            var projects = new[] {A.Dummy<ISolutionProject>(), A.Dummy<ISolutionProject>()};
            A.CallTo(() => this.solutionFilesService.Projects).Returns(projects);

            this.eventListener.Initialize("");

            A.CallTo(() => this.fileMonitor.MonitorProject(A<string>._)).MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}
