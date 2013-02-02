namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    using System.Linq;
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

        public void ActOn(IEnumerable<string> files, Trigger trigger)
        {
            throw new System.NotImplementedException();
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
