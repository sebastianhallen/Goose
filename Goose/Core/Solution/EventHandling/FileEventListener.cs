namespace Goose.Core.Solution.EventHandling
{
    using System.Collections.Generic;
    using System.Linq;
    using Dispatcher;
    using Goose.Core.Configuration;
    using Goose.Core.Solution;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using OnSaveTask;

    public class FileEventListener
        : IFileChangeConsumer
    {
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IFileMonitor fileMonitor;
        private readonly IOnChangeTaskDispatcher taskDispatcher;
        private readonly IGooseActionFactory actionFactory;
        private ActionConfiguration configuration;

        public FileEventListener(ISolutionFilesService solutionFilesService, IFileMonitor fileMonitor, IOnChangeTaskDispatcher taskDispatcher, IGooseActionFactory actionFactory, IFileChangeSubscriber fileChangeSubscriber)
        {
            this.solutionFilesService = solutionFilesService;
            this.fileMonitor = fileMonitor;
            this.taskDispatcher = taskDispatcher;
            this.actionFactory = actionFactory;

            fileChangeSubscriber.Attach(this);
        }

        public IOnChangeTaskDispatcher TaskDispatcher
        {
            get { return this.taskDispatcher; }
        }

        public void Initialize(ActionConfiguration watchConfiguration)
        {
            this.configuration = watchConfiguration;

            if (this.configuration.IsValid)
            {
                this.configuration = watchConfiguration;
                foreach (var project in this.solutionFilesService.Projects)
                {
                    this.fileMonitor.MonitorProject(project.ProjectFilePath, watchConfiguration.Glob);
                }
            } 
        }

        public void ActOn(IEnumerable<string> files, Trigger trigger)
        {
            var isFileUpdate = files.Any(this.fileMonitor.IsMonitoredFile);

            this.UpdateMonitors(files, trigger);

            if ((Trigger.Save.Equals(trigger)) ||
                (Trigger.Delete.Equals(trigger) && isFileUpdate))
            {
                var action = this.actionFactory.Create(this.configuration);
                this.taskDispatcher.QueueOnChangeTask(action);
            }
        }

        private void UpdateMonitors(IEnumerable<string> files, Trigger trigger)
        {
            if (Trigger.Delete.Equals(trigger))
            {
                this.fileMonitor.UnMonitor(files);
            }

            if (Trigger.Save.Equals(trigger))
            {
                var monitoredProjects = files.Where(this.fileMonitor.IsMonitoredProject).ToArray();
                this.fileMonitor.UnMonitor(monitoredProjects);
                foreach (var project in monitoredProjects)
                {
                    this.fileMonitor.MonitorProject(project, this.configuration.Glob);
                }
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
