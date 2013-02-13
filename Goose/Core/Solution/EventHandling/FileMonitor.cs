namespace Goose.Core.Solution.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Debugging;
    using Output;

    public class FileMonitor
        : IFileMonitor
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IGlobMatcher globMatcher;
        private readonly IFileChangeSubscriber fileChangeSubscriber;
        private readonly IOutputService outputService;
        private readonly IList<MonitoredFile> monitoredFilesField;
        private readonly IList<MonitoredFile> monitoredProjectsField;
        
        public FileMonitor(ISolutionFilesService solutionFilesService, IGlobMatcher globMatcher, IFileChangeSubscriber fileChangeSubscriber, IOutputService outputService){
            this.solutionFilesService = solutionFilesService;
            this.globMatcher = globMatcher;
            this.fileChangeSubscriber = fileChangeSubscriber;
            this.outputService = outputService;
            this.monitoredFilesField = new List<MonitoredFile>();
            this.monitoredProjectsField = new List<MonitoredFile>();
        }

        public void MonitorProject(string path, string glob)
        {
            var matchingFilesInProject = 
                (from project in this.solutionFilesService.Projects
                let projectPath = project.ProjectFilePath
                where !string.IsNullOrWhiteSpace(projectPath) && path.Equals(projectPath)
                from file in project.Files
                where this.globMatcher.Matches(file.FilePath, glob)
                select file).ToArray();

            this.monitoredProjectsField.Add(this.fileChangeSubscriber.Subscribe(path, path));
            this.outputService.Debug<FileMonitor>("MonitorProject: project: " + path);
            foreach (var file in matchingFilesInProject)
            {
                this.monitoredFilesField.Add(this.fileChangeSubscriber.Subscribe(path, file.FilePath));
            }
            this.outputService.Debug<FileMonitor>("Monitored files in project: " + string.Join(Environment.NewLine, matchingFilesInProject.Select(file => file.ToString())));
        }

        public void UnMonitor(IEnumerable<string> files)
        {
            var filesToUnMonitor = files.ToArray();
            if (!filesToUnMonitor.Any()) return;

            UnMonitorProject(filesToUnMonitor);
            UnMonitorFile(filesToUnMonitor);
        }

        private void UnMonitorProject(IEnumerable<string> files)
        {
            var matchingProjects = this.monitoredProjectsField.Where(project => files.Contains(project.FilePath));
            var filesInProjects = this.monitoredFilesField.Where(file =>
            {
                var projectFiles = matchingProjects.Select(project => project.FilePath);
                return projectFiles.Contains(file.ProjectPath);
            }).ToArray();

            if (filesInProjects.Any())
            {
                this.UnMonitorFile(filesInProjects.Select(file => file.FilePath));
            }
                        
            foreach (var project in matchingProjects.ToArray())
            {                
                this.outputService.Debug<FileMonitor>("Unsubscring from project: " + project);
                this.fileChangeSubscriber.UnSubscribe(project.MonitorCookie);
                this.monitoredProjectsField.Remove(project);
            }
        }

        private void UnMonitorFile(IEnumerable<string> files)
        {
            this.outputService.Debug<FileMonitor>("unmonitor called for: " + string.Join(Environment.NewLine, files));
            var matchingFiles = this.monitoredFilesField
                                  .Where(file => files.Contains(file.FilePath));

            foreach (var file in matchingFiles.ToArray())
            {                
                this.fileChangeSubscriber.UnSubscribe(file.MonitorCookie);
                this.monitoredFilesField.Remove(file);
            }

            this.outputService.Debug<FileMonitor>("Unsubscribed from files: " + string.Join(Environment.NewLine, matchingFiles));
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
