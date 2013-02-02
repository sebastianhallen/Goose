namespace Goose.Tests.EventHandling
{
    using Goose.Core.Configuration;
    using Goose.Core.Solution;
    using Goose.Core.Solution.EventHandling;
    using FakeItEasy;
    using NUnit.Framework;

    [TestFixture]
    public class FileMonitorTests
    {
        [UnderTest] private FileMonitor fileMonitor;
        [Fake] private ISolutionFilesService solutionFilesService;
        [Fake] private IFileChangeSubscriber fileChangeSubscriber;
        [Fake] private IGlobMatcher globMatcher;

        private FakeSolutionTestContext solution;
        
        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
            A.CallTo(() => this.globMatcher.Matches(A<string>._, A<string>._)).Returns(true);
            this.solution = new FakeSolutionTestContext(this.solutionFilesService);

        }

        [Test]
        public void Should_monitor_matching_files_in_project_when_monitoring_project()
        {            
            this.solution.HasProject("project.csproj").WithFiles("file.less", "other.less");
            this.solution.Construct();

            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<ActionConfiguration>());

            A.CallTo(() => this.fileChangeSubscriber.Watch("file.less")).MustHaveHappened();
            A.CallTo(() => this.fileChangeSubscriber.Watch("other.less")).MustHaveHappened();
        }

        [Test]
        public void Should_monitor_project_file_when_monitoring_project()
        {
            this.solution.HasProject("project.csproj");
            this.solution.Construct();

            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<ActionConfiguration>());

            A.CallTo(() => this.fileChangeSubscriber.Watch("project.csproj")).MustHaveHappened();
        }

        [Test]
        public void File_change_subscriber_should_be_attached_to_monitor()
        {
            A.CallTo(() => this.fileChangeSubscriber.Attach(this.fileMonitor)).MustHaveHappened();
        }        
    }
}
