namespace Goose.Tests.EventHandling
{
    using System.Collections.Generic;
    using Core.Solution;
    using Core.Solution.EventHandling;
    using FakeItEasy;

    public class FakeSolutionTestContext
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IFileChangeSubscriber fileChangeSubscriber;
        private readonly IList<ISolutionProject> projects;
        public FakeSolutionTestContext(ISolutionFilesService solutionFilesService, IFileChangeSubscriber fileChangeSubscriber)
        {
            this.solutionFilesService = solutionFilesService;
            this.fileChangeSubscriber = fileChangeSubscriber;
            this.projects = new List<ISolutionProject>();
        }

        public FakeProjectContext HasProject(string path)
        {
            var project = A.Fake<ISolutionProject>();
            A.CallTo(() => project.ProjectFilePath).Returns(path);
            this.projects.Add(project);
            return new FakeProjectContext(project, this.fileChangeSubscriber);
        }

        public IEnumerable<uint> Construct()
        {
            uint cookie = 100;
            var cookies = new List<uint>();
            foreach (var project in projects)
            {
                cookies.Add(++cookie);
                A.CallTo(() => this.fileChangeSubscriber.Subscribe(project.ProjectFilePath, project.ProjectFilePath))
                    .Returns(new MonitoredFile(cookie, project.ProjectFilePath, project.ProjectFilePath));
            }
            A.CallTo(() => this.solutionFilesService.Projects).Returns(this.projects);
            return cookies;
        }
    }
}