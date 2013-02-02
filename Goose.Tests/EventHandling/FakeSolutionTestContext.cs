namespace Goose.Tests.EventHandling
{
    using System.Collections.Generic;
    using Core.Solution;
    using FakeItEasy;

    public class FakeSolutionTestContext
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IList<ISolutionProject> projects;
        public FakeSolutionTestContext(ISolutionFilesService solutionFilesService)
        {
            this.solutionFilesService = solutionFilesService;
            this.projects = new List<ISolutionProject>();
        }

        public FakeProjectContext HasProject(string path)
        {
            var project = A.Fake<ISolutionProject>();
            A.CallTo(() => project.ProjectFilePath).Returns(path);
            this.projects.Add(project);
            return new FakeProjectContext(project);
        }

        public void Construct()
        {
            A.CallTo(() => this.solutionFilesService.Projects).Returns(this.projects);
        }
    }
}