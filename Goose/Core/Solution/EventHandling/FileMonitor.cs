namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    using System.Linq;

    public class FileMonitor
        : IFileMonitor
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IGlobMatcher globMatcher;
        private readonly IFileChangeSubscriber fileChangeSubscriber;
        private readonly IList<MonitoredFile> monitoredFilesField;
        private readonly IList<MonitoredFile> monitoredProjectsField;
        
        public FileMonitor(ISolutionFilesService solutionFilesService, IGlobMatcher globMatcher, IFileChangeSubscriber fileChangeSubscriber)
        {
            this.solutionFilesService = solutionFilesService;
            this.globMatcher = globMatcher;
            this.fileChangeSubscriber = fileChangeSubscriber;
            this.monitoredFilesField = new List<MonitoredFile>();
            this.monitoredProjectsField = new List<MonitoredFile>();
        }

        public void MonitorProject(string path, string glob)
        {
            var matchingFilesInProject = 
                from project in this.solutionFilesService.Projects.Where(project => project.ProjectFilePath.Equals(path))
                from file in project.Files
                where this.globMatcher.Matches(file.FilePath, glob)
                select file;

            this.monitoredProjectsField.Add(this.fileChangeSubscriber.Subscribe(path, path));
            foreach (var file in matchingFilesInProject)
            {
                this.monitoredFilesField.Add(this.fileChangeSubscriber.Subscribe(path, file.FilePath));
            }
        }

        public void UnMonitor(IEnumerable<string> files)
        {
            UnMonitorProject(files);
            UnMonitorFile(files);
        }

        private void UnMonitorProject(IEnumerable<string> files)
        {
            var matchingProjects = this.monitoredProjectsField.Where(project => files.Contains(project.FilePath));
            var filesInProjects = this.monitoredFilesField.Where(file =>
            {
                var projectFiles = matchingProjects.Select(project => project.FilePath);
                return projectFiles.Contains(file.ProjectPath);
            });

            this.UnMonitorFile(filesInProjects.Select(file => file.FilePath));            
            foreach (var project in matchingProjects.ToArray())
            {
                this.fileChangeSubscriber.UnSubscribe(project.MonitorCookie);
                this.monitoredProjectsField.Remove(project);
            }
        }

        private void UnMonitorFile(IEnumerable<string> files)
        {
            var matchingFiles = this.monitoredFilesField
                                  .Where(file => files.Contains(file.FilePath));

            foreach (var file in matchingFiles.ToArray())
            {
                this.fileChangeSubscriber.UnSubscribe(file.MonitorCookie);
                this.monitoredFilesField.Remove(file);
            }
        }

        public bool IsMonitoredProject(string project)
        {
            return this.monitoredProjectsField.Any(monitored => monitored.ProjectPath.Equals(project));
        }

        public bool IsMonitoredFile(string file)
        {
            return this.monitoredFilesField.Any(monitored => monitored.FilePath.Equals(file));
        }

        public void Dispose()
        {
            var files = this.monitoredFilesField.Select(file => file.FilePath);
            var projects = this.monitoredProjectsField.Select(project => project.FilePath);
            this.UnMonitor(files);
            this.UnMonitor(projects);
        }
    }
}
