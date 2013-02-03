namespace Goose.Tests.EventHandling
{
    using System.Linq;
    using FakeItEasy;
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

            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<string>());

            A.CallTo(() => this.fileChangeSubscriber.Subscribe("project.csproj", "file.less")).MustHaveHappened();
            A.CallTo(() => this.fileChangeSubscriber.Subscribe("project.csproj", "other.less")).MustHaveHappened();
        }

        [Test]
        public void Should_monitor_project_file_when_monitoring_project()
        {
            this.solution.HasProject("project.csproj");
            this.solution.Construct();

            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<string>());

            A.CallTo(() => this.fileChangeSubscriber.Subscribe("project.csproj", "project.csproj")).MustHaveHappened();
        }

        [Test]
        public void Should_cancel_subscription_when_file_is_deleted()
        {
            var subscriptions = this.solution.HasProject("project.csproj").WithFiles("file");
            this.solution.Construct();
            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<string>());

            this.fileMonitor.UnMonitor(new[] { "file" });

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
            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<string>());

            this.fileMonitor.UnMonitor(new[] { "project.csproj" });

            foreach (var subscription in subscriptions)
            {
                A.CallTo(() => this.fileChangeSubscriber.UnSubscribe(subscription)).MustHaveHappened();
            }
        }

        [Test]
        public void Should_unsubscribe_to_all_files_in_project_when_disposing()
        {
            var subscriptions = this.solution.HasProject("project.csproj").WithFiles("file", "other file");
            this.solution.Construct();
            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<string>());

            this.fileMonitor.Dispose();

            foreach (var cookie in subscriptions)
            {
                A.CallTo(() => this.fileChangeSubscriber.UnSubscribe(cookie)).MustHaveHappened();
            }            
        }

        [Test]
        public void Should_unsubscribe_to_project_file_when_disposing()
        {
            this.solution.HasProject("project.csproj").WithFiles("file", "other file");
            var subscriptions = this.solution.Construct();
            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<string>());

            this.fileMonitor.Dispose();

            foreach (var cookie in subscriptions)
            {
                A.CallTo(() => this.fileChangeSubscriber.UnSubscribe(cookie)).MustHaveHappened();
            }
        }

        [Test]
        public void Should_not_call_unsubscribe_on_a_file_twice()
        {
            this.solution.HasProject("project.csproj").WithFiles("file");
            this.solution.Construct();
            this.fileMonitor.MonitorProject("project.csproj", A.Dummy<string>());

            this.fileMonitor.UnMonitor(new [] { "project.csproj", "file"});
            this.fileMonitor.UnMonitor(new[] { "project.csproj", "file" });

            A.CallTo(() => this.fileChangeSubscriber.UnSubscribe(A<uint>._)).MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}
