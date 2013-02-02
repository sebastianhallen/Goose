namespace Goose.Tests.EventHandling
{
    using System.Linq;
    using Core.Solution;
    using FakeItEasy;

    public class FakeProjectContext
    {
        private readonly ISolutionProject project;

        public FakeProjectContext(ISolutionProject project)
        {
            this.project = project;
        }

        public void WithFiles(params string[] files)
        {
            A.CallTo(() => this.project.Files).Returns(
                files.Select(file => new FileInProject("", file, 0))
                );
        }
    }
}