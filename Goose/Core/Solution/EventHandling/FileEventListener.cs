namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    using System.Linq;
    using Goose.Core.Configuration;
    using Goose.Core.Solution;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    public class FileEventListener
        : IFileChangeConsumer
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IFileMonitor fileMonitor;
        
        public FileEventListener(ISolutionFilesService solutionFilesService, IFileMonitor fileMonitor, IFileChangeSubscriber fileChangeSubscriber)
        {
            this.solutionFilesService = solutionFilesService;
            this.fileMonitor = fileMonitor;
            
            fileChangeSubscriber.Attach(this);
        }

        public void Initialize(ActionConfiguration watchConfiguration)
        {
            foreach (var project in this.solutionFilesService.Projects)
            {
                this.fileMonitor.MonitorProject(project.ProjectFilePath, watchConfiguration.Glob);
            }
        }

        public void ActOn(IEnumerable<string> files, Trigger trigger)
        {
            if (Trigger.Delete.Equals(trigger))
            {
                this.fileMonitor.UnMonitor(files);
            }
        }

        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            var deleteMask = (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del;
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
