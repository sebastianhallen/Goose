namespace Goose.Core.EventListener
{
    using System.Threading.Tasks;
    using Configuration;
    using Solution;
    using Solution.EventHandling;

    public class FileEventListener
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IFileMonitor fileMonitor;
        private readonly IGooseTaskFactory taskFactory;

        public FileEventListener(ISolutionFilesService solutionFilesService, IFileMonitor fileMonitor, IGooseTaskFactory taskFactory)
        {
            this.solutionFilesService = solutionFilesService;
            this.fileMonitor = fileMonitor;
            this.taskFactory = taskFactory;
        }

        public void Initialize(ActionConfiguration watchConfiguration)
        {
            foreach (var project in this.solutionFilesService.Projects)
            {
                var triggeredTask = this.taskFactory.CreateTask(watchConfiguration);
                this.fileMonitor.MonitorProject(project.ProjectFilePath, triggeredTask);
            }
        }
    }

    public interface IGooseTaskFactory
    {
        IGooseAction CreateTask(ActionConfiguration actionConfiguration);
    }
}
