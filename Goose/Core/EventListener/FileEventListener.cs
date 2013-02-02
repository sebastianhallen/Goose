
namespace Goose.Core.EventListener
{
    using System.Linq;
    using Solution;

    public class FileEventListener
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IFileMonitor fileMonitor;
        private readonly IGlobMatcher globMatcher;

        public FileEventListener(ISolutionFilesService solutionFilesService, IFileMonitor fileMonitor, IGlobMatcher globMatcher)
        {
            this.solutionFilesService = solutionFilesService;
            this.fileMonitor = fileMonitor;
            this.globMatcher = globMatcher;
        }

        public void Initialize(string glob)
        {
            this.MonitorFileChanges(glob);
        }

        private void MonitorFileChanges(string glob)
        {
            var matchingFilesInProject = 
                from project in this.solutionFilesService.Projects
                from file in project.Files
				where this.globMatcher.Matches(file.FilePath, glob)
				select file;

            foreach (var file in matchingFilesInProject)
            {
                this.fileMonitor.MonitorFile(file);
            }
        }
    }
}
