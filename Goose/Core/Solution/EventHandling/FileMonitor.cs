namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    using System.Linq;
    using Dispatcher;
    using Goose.Core.Configuration;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    public class FileMonitor
        : IFileMonitor, IFileChangeConsumer
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

            this.fileChangeSubscriber.Attach(this);
        }

        public void MonitorProject(string path, IGooseAction triggeredAction)
        {
            var matchingFilesInProject = 
                from project in this.solutionFilesService.Projects.Where(project => project.ProjectFilePath.Equals(path))
                from file in project.Files
                where this.globMatcher.Matches(file.FilePath, triggeredAction.Glob)
                select file;

            this.monitoredProjectsField.Add(this.fileChangeSubscriber.Subscribe(path, path));
            foreach (var file in matchingFilesInProject)
            {
                this.monitoredFilesField.Add(this.fileChangeSubscriber.Subscribe(path, file.FilePath));
            }
        }

        public void ActOn(IEnumerable<string> files, Trigger trigger)
        {
            if (Trigger.Delete.Equals(trigger))
            {
                var projectCookies = this.monitoredProjectsField
                                         .Where(project => files.Contains(project.FilePath))
                                         .SelectMany(project =>
                                         {
                                             var cookies = new[] {project.MonitorCookie};
                                             return cookies.Concat(
                                                 this.monitoredFilesField
                                                     .Where(file => file.ProjectPath.Equals(project.FilePath))
                                                     .Select(file => file.MonitorCookie));
                                         });

                var fileCookies = this.monitoredFilesField
                                  .Where(file => files.Contains(file.FilePath))
                                  .Select(file => file.MonitorCookie);

                foreach (var cookie in projectCookies.Concat(fileCookies))
                {
                    this.fileChangeSubscriber.UnSubscribe(cookie);
                }                
            }
        }

        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            var deleteMask = (uint) _VSFILECHANGEFLAGS.VSFILECHG_Del;
            var changeMask = (uint)_VSFILECHANGEFLAGS.VSFILECHG_Add | (uint)_VSFILECHANGEFLAGS.VSFILECHG_Size | (uint)_VSFILECHANGEFLAGS.VSFILECHG_Time;
            if (rggrfChange.Any(change => (change & deleteMask) == deleteMask))
            {
                this.ActOn(rgpszFile, Trigger.Delete);
            }

            if (rggrfChange.Any(change => (change & changeMask) != 0x00))
            {
                this.ActOn(rgpszFile, Trigger.Save);
            }

            return VSConstants.S_OK;
        }

        public int DirectoryChanged(string pszDirectory)
        {
            return VSConstants.S_OK;
        }
    }
}
