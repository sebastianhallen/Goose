namespace Goose.Core.EventListener
{
    using Configuration;
    using Solution;
    using Solution.EventHandling;

    public class FileEventListener
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IFileMonitor fileMonitor;
        
        public FileEventListener(ISolutionFilesService solutionFilesService, IFileMonitor fileMonitor, IGlobMatcher globMatcher)
        {
            this.solutionFilesService = solutionFilesService;
            this.fileMonitor = fileMonitor;
        }

        public void Initialize(ActionConfiguration watchConfiguration)
        {
            this.MonitorFileChanges(watchConfiguration);
        }

        private void MonitorFileChanges(ActionConfiguration watchConfiguration)
        {
            foreach (var project in this.solutionFilesService.Projects)
            {
                this.fileMonitor.MonitorProject(project.ProjectFilePath, watchConfiguration);
            }
        }
    }
}
