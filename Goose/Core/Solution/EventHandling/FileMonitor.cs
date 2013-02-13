namespace Goose.Core.Solution.EventHandling
{
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
        private readonly ConcurrentDictionary<string, MonitoredFile> monitoredProjectsField;
        
        public FileMonitor(ISolutionFilesService solutionFilesService, IGlobMatcher globMatcher, IFileChangeSubscriber fileChangeSubscriber, IOutputService outputService){
            this.solutionFilesService = solutionFilesService;
            this.globMatcher = globMatcher;
            this.fileChangeSubscriber = fileChangeSubscriber;
            this.outputService = outputService;
            this.monitoredFilesField = new ConcurrentDictionary<string, MonitoredFile>();
            this.monitoredProjectsField = new ConcurrentDictionary<string, MonitoredFile>();
        }

        public void MonitorProject(string path, string glob)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            var projectCandidatePath = path;
            var matchingFilesInProject = 
                (from project in this.solutionFilesService.Projects
                 let projectPath = project.ProjectFilePath
                 where !string.IsNullOrWhiteSpace(projectPath) && projectCandidatePath.Equals(projectPath)
                    from file in project.Files
                    where this.globMatcher.Matches(file.FilePath, glob)
                        select file).ToArray();

            
            this.monitoredProjectsField.AddOrUpdate(path,
                projectPath => this.fileChangeSubscriber.Subscribe(projectPath, projectPath),
                (projectPath, existing) => existing);
            this.outputService.Debug<FileMonitor>("subscribed to project: " + path);

            foreach (var file in matchingFilesInProject)
            {                
                this.monitoredFilesField.AddOrUpdate(file.FilePath, 
                    filePath => this.fileChangeSubscriber.Subscribe(path, filePath), 
                    (filePath, existing) => existing);
                this.outputService.Debug<FileMonitor>("subscribed to file: " + file);

            }
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
            foreach (var file in files)
            {
                MonitoredFile monitoredProject;
                if (this.monitoredProjectsField.TryRemove(file, out monitoredProject))
                {
                    var filesInProject = this.monitoredFilesField
                                             .Where(monitoredFile => monitoredFile.Value.ProjectPath.Equals(monitoredProject.ProjectPath))
                                             .Select(monitoredFile => monitoredFile.Key);
                    this.UnMonitorFile(filesInProject);
                    this.fileChangeSubscriber.UnSubscribe(monitoredProject.MonitorCookie);
                    this.outputService.Debug<FileMonitor>("Unsubscribed from project: " + file);

                }
            }
        }

        private void UnMonitorFile(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                MonitoredFile monitoredFile;
                if (this.monitoredFilesField.TryRemove(file, out monitoredFile))
                {
                    this.fileChangeSubscriber.UnSubscribe(monitoredFile.MonitorCookie);
                    this.outputService.Debug<FileMonitor>("Unsubscribed from file: " + file);
                }                
            }                                    
        }

        public bool IsMonitoredProject(string project)
        {
            return this.monitoredProjectsField.ContainsKey(project);
        }

        public bool IsMonitoredFile(string file)
        {
            return this.monitoredFilesField.ContainsKey(file);
        }

        public void Dispose()
        {
            var monitoredFiles = this.monitoredFilesField.Keys;
            var monitoredProjects = this.monitoredProjectsField.Keys;
            this.UnMonitor(monitoredFiles);
            this.UnMonitor(monitoredProjects);
        }
    }
}
