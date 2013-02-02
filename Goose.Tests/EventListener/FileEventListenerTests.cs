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
        public void Should_add_all_project_files_with_matching_file_name_to_monitored_files_when_initialized()
        {
            var project = A.Fake<ISolutionProject>();
            A.CallTo(() => project.Files).Returns(new[] {A.Dummy<ProjectFile>(), A.Dummy<ProjectFile>()});
            A.CallTo(() => this.solutionFilesService.Projects).Returns(new[] { project });

            this.eventListener.Initialize("");

            A.CallTo(() => this.fileMonitor.MonitorFile(A<ProjectFile>._)).MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}
