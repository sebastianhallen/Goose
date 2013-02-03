namespace Goose.Tests.EventHandling
{
    using System.Threading.Tasks;
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
        [Fake] private IGooseTaskFactory taskFactory;
        [Fake] private IFileChangeSubscriber fileChangeSubscriber;
        private FakeSolutionTestContext solution;
        
        [SetUp]
        public void Before()
        {
            Fake.InitializeFixture(this);
            this.solution = new FakeSolutionTestContext(this.solutionFilesService, this.fileChangeSubscriber);
        }

        [Test]
        public void Should_monitor_all_project_files_in_solution_when_initialized()
        {
            this.solution.HasProject("web.csproj");
            this.solution.HasProject("business.csproj");
            this.solution.Construct();

            this.eventListener.Initialize(A.Dummy<ActionConfiguration>());

            A.CallTo(() => this.fileMonitor.MonitorProject("web.csproj", A<IGooseAction>._)).MustHaveHappened();
            A.CallTo(() => this.fileMonitor.MonitorProject("business.csproj", A<IGooseAction>._)).MustHaveHappened();
        }

        [Test]
        public void Should_create_task_from_configuration_when_initializing()
        {
            this.solution.HasProject("project.csproj");
            this.solution.Construct();

            this.eventListener.Initialize(A.Dummy<ActionConfiguration>());

            A.CallTo(() => this.taskFactory.CreateTask(A<ActionConfiguration>._)).MustHaveHappened();
        }
    }
}
