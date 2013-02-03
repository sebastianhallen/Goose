namespace Goose.Tests.EventHandling
{
    using System.Linq;
    using FakeItEasy;
    using Goose.Core.Configuration;
    using Goose.Core.Solution;
    using Goose.Core.Solution.EventHandling;
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
            this.solution = new FakeSolutionTestContext(this.solutionFilesService, this.fileChangeSubscriber);

        }

        [Test]
        public void Should_monitor_matching_files_in_project_when_monitoring_project()
        {            
            this.solution.HasProject("project.csproj").WithFiles("file.less", "other.less");
            this.solution.Construct();

            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<IGooseAction>());

            A.CallTo(() => this.fileChangeSubscriber.Subscribe("project.csproj", "file.less")).MustHaveHappened();
            A.CallTo(() => this.fileChangeSubscriber.Subscribe("project.csproj", "other.less")).MustHaveHappened();
        }

        [Test]
        public void Should_monitor_project_file_when_monitoring_project()
        {
            this.solution.HasProject("project.csproj");
            this.solution.Construct();

            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<IGooseAction>());

            A.CallTo(() => this.fileChangeSubscriber.Subscribe("project.csproj", "project.csproj")).MustHaveHappened();
        }

        [Test]
        public void File_change_subscriber_should_be_attached_to_monitor()
        {
            A.CallTo(() => this.fileChangeSubscriber.Attach(this.fileMonitor)).MustHaveHappened();
        }

        [Test]
        public void Should_cancel_subscription_when_file_is_deleted()
        {
            var subscriptions = this.solution.HasProject("project.csproj").WithFiles("file");
            this.solution.Construct();
            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<IGooseAction>());

            this.fileMonitor.ActOn(new [] {"file"}, Trigger.Delete);

            foreach (var subscription in subscriptions)
            {
                A.CallTo(() => this.fileChangeSubscriber.UnSubscribe(subscription)).MustHaveHappened();   
            }            
        }

        [Test]
        public void Should_cancel_subscription_to_all_files_in_project_when_project_file_is_deleted()
        {
            var subscriptions = this.solution.HasProject("project.csproj").WithFiles("file", "another");
            subscriptions = subscriptions.Concat(this.solution.Construct());
            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<IGooseAction>());

            this.fileMonitor.ActOn(new[] { "project.csproj" }, Trigger.Delete);

            foreach (var subscription in subscriptions)
            {
                A.CallTo(() => this.fileChangeSubscriber.UnSubscribe(subscription)).MustHaveHappened();
            }   
        }
    }
}
