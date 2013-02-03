namespace Goose.Tests.EventHandling
{
    using System.Collections.Generic;
    using Core.Solution;
    using Core.Solution.EventHandling;
    using FakeItEasy;

    public class FakeProjectContext
    {
        private readonly ISolutionProject project;
        private readonly IFileChangeSubscriber fileChangeSubscriber;
 
        public FakeProjectContext(ISolutionProject project, IFileChangeSubscriber fileChangeSubscriber)
        {
            this.project = project;
            this.fileChangeSubscriber = fileChangeSubscriber;
        }

        public IEnumerable<uint> WithFiles(params string[] filesInProject)
        {
            uint fileIdIndexer = 0; 
            var files = new List<FileInProject>();
            var fileIds = new List<uint>();
            foreach (var file in filesInProject)
            {
                fileIds.Add(++fileIdIndexer);
                A.CallTo(() => this.fileChangeSubscriber.Subscribe(this.project.ProjectFilePath, file))
                    .Returns(new MonitoredFile(fileIdIndexer, this.project.ProjectFilePath, file));
                files.Add(new FileInProject("", file, fileIdIndexer));                
            }
            A.CallTo(() => this.project.Files).Returns(files);
            return fileIds;
        }
    }
}