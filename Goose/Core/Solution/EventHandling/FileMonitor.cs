namespace Goose.Core.Solution.EventHandling
{
    using System;
    using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, MonitoredFile> monitoredFilesField;
        private readonly IList<MonitoredFile> monitoredProjectsField;
        
        public FileMonitor(ISolutionFilesService solutionFilesService, IGlobMatcher globMatcher, IFileChangeSubscriber fileChangeSubscriber, IOutputService outputService){
            this.solutionFilesService = solutionFilesService;
            this.globMatcher = globMatcher;
            this.fileChangeSubscriber = fileChangeSubscriber;
            this.outputService = outputService;
            this.monitoredFilesField = new ConcurrentDictionary<string, MonitoredFile>();
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
                this.monitoredFilesField.AddOrUpdate(file.FilePath, 
                    filePath => this.fileChangeSubscriber.Subscribe(path, filePath), 
                    (filePath, existing) => existing);
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

                return projectFiles.Contains(file.Value.ProjectPath);
            }).ToArray();

            if (filesInProjects.Any())
            {
                this.UnMonitorFile(filesInProjects.Select(file => file.Key));
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
            foreach (var file in files)
            {
                MonitoredFile monitoredFile;
                if (this.monitoredFilesField.TryRemove(file, out monitoredFile))
                {
                    this.fileChangeSubscriber.UnSubscribe(monitoredFile.MonitorCookie);
                    this.outputService.Debug<FileMonitor>("Unsubscribed from: " + file);
                }                
            }                                    
        }

        public bool IsMonitoredProject(string project)
        {
            return this.monitoredProjectsField.Any(monitored => monitored.ProjectPath.Equals(project));
        }

        public bool IsMonitoredFile(string file)
        {
            return this.monitoredFilesField.ContainsKey(file);
        }

        public void Dispose()
        {
            var monitoredFiles = this.monitoredFilesField.Keys;
            
            var projects = this.monitoredProjectsField.Select(project => project.FilePath);
            this.UnMonitor(monitoredFiles);
            this.UnMonitor(projects);
        }
    }
}
