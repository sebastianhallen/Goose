namespace Goose.Tests.EventHandling
{
    using Goose.Core.Configuration;
    using Goose.Core.EventListener;
    using Goose.Core.Solution;
    using Goose.Core.Solution.EventHandling;
    using FakeItEasy;
    using NUnit.Framework;

    [TestFixture]
    public class FileEventListenerTests
    {
        [UnderTest] private FileEventListener eventListener;
        [Fake] private ISolutionFilesService solutionFilesService;
        [Fake] private IFileMonitor fileMonitor;
        private FakeSolutionTestContext solution;
        
        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
            this.solution = new FakeSolutionTestContext(this.solutionFilesService);
        }

        [Test]
        public void Should_monitor_all_project_files_in_solution_when_initialized()
        {
            this.solution.HasProject("web.csproj");
            this.solution.HasProject("business.csproj");
            this.solution.Construct();

            this.eventListener.Initialize(A.Dummy<ActionConfiguration>());

            A.CallTo(() => this.fileMonitor.MonitorProject("web.csproj", A<ActionConfiguration>._)).MustHaveHappened();
            A.CallTo(() => this.fileMonitor.MonitorProject("business.csproj", A<ActionConfiguration>._)).MustHaveHappened();
        }
    }
}
