namespace Goose.Core.Solution
{
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Microsoft.VisualStudio.Shell.Interop;

    public interface IFileChangeSubscriber
    {
        MonitoredFile Watch(string file);
        void Attach(FileMonitor fileMonitor);
    }

    public class FileChangeMonitor
        
    {
        private readonly IVsFileChangeEx fileChangeEx;

        public FileChangeMonitor(IVsFileChangeEx fileChangeEx)
        {
            this.fileChangeEx = fileChangeEx;
        }
    }

    public class FileMonitor
        : IFileMonitor
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IGlobMatcher globMatcher;
        private readonly IFileChangeSubscriber fileChangeSubscriber;
        public readonly IList<MonitoredFile> monitoredFilesField;
        public readonly IList<MonitoredFile> monitoredProjectsField;
        
        public FileMonitor(ISolutionFilesService solutionFilesService, IGlobMatcher globMatcher, IFileChangeSubscriber fileChangeSubscriber)
        {
            this.solutionFilesService = solutionFilesService;
            this.globMatcher = globMatcher;
            this.fileChangeSubscriber = fileChangeSubscriber;
            this.monitoredFilesField = new List<MonitoredFile>();
            this.monitoredProjectsField = new List<MonitoredFile>();

            this.fileChangeSubscriber.Attach(this);
        }

        public void MonitorFile(FileInProject file, Trigger trigger)
        {
            this.monitoredFilesField.Add(this.fileChangeSubscriber.Watch(file.FilePath));
        }

        public void MonitorProject(string path, ActionConfiguration watchConfiguration)
        {
            var matchingFilesInProject = 
                from project in this.solutionFilesService.Projects.Where(project => project.ProjectFilePath.Equals(path))
                from file in project.Files
                where this.globMatcher.Matches(file.FilePath, watchConfiguration.Glob)
                select file;

            this.monitoredProjectsField.Add(this.fileChangeSubscriber.Watch(path));
            foreach (var file in matchingFilesInProject)
            {
                this.MonitorFile(file, watchConfiguration.Trigger);
            }
        }

        public bool IsMonitoredProject(string projectCandidate)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateFileMonitorsForProject(string path)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<MonitoredFile> MonitoredFiles
        {
            get { return this.monitoredFilesField; }
        }

        public IEnumerable<MonitoredFile> MonitoredProjects
        {
            get { return this.monitoredProjectsField; }
        }
    }
}
