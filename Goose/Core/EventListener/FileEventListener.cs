namespace Goose.Core.EventListener
{
    using System.Linq;
    using Configuration;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Solution;

    public class FileEventListener
        : IVsFileChangeEvents
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

        public void Initialize(ActionConfiguration watchConfiguration)
        {
            this.MonitorFileChanges(watchConfiguration);
        }

        private void MonitorFileChanges(ActionConfiguration watchConfiguration)
        {
            var matchingFilesInProject = 
                from project in this.solutionFilesService.Projects
                from file in project.Files
				where this.globMatcher.Matches(file.FilePath, watchConfiguration.Glob)
				select file;

            foreach (var file in matchingFilesInProject)
            {
                this.fileMonitor.MonitorFile(file, watchConfiguration.Trigger);
            }

            foreach (var project in this.solutionFilesService.Projects)
            {
                this.fileMonitor.MonitorProject(project.ProjectFilePath, watchConfiguration);
            }
        }

        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            foreach (var file in rgpszFile)
            {
                if (this.fileMonitor.IsMonitoredProject(file))
                {
                    this.fileMonitor.UpdateFileMonitorsForProject(file);
                }
            }
            return VSConstants.S_OK;
        }

        public int DirectoryChanged(string pszDirectory)
        {
            return VSConstants.S_OK;
        }
    }
}
