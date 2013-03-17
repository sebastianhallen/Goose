namespace Goose.Core.Solution.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Action;
    using Dispatcher;
    using Goose.Core.Configuration;
    using Goose.Core.Solution;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    public class FileEventListener
        : IFileChangeConsumer, IDisposable
    {
        private readonly IFileMonitor fileMonitor;
        private readonly IOnChangeTaskDispatcher taskDispatcher;
        private readonly IGooseActionFactory actionFactory;
        private ActionConfiguration configuration;

        public FileEventListener(IFileMonitor fileMonitor, IOnChangeTaskDispatcher taskDispatcher, IGooseActionFactory actionFactory, IFileChangeSubscriber fileChangeSubscriber)
        {
            this.fileMonitor = fileMonitor;
            this.taskDispatcher = taskDispatcher;
            this.actionFactory = actionFactory;

            fileChangeSubscriber.Attach(this);
        }

        public IOnChangeTaskDispatcher TaskDispatcher
        {
            get { return this.taskDispatcher; }
        }

        public void Initialize(ISolutionProject project, ActionConfiguration watchConfiguration)
        {
            this.configuration = watchConfiguration;

            var projectPath = project.ProjectFilePath;
            if (this.configuration.IsValid && !string.IsNullOrWhiteSpace(projectPath))
            {
                this.configuration = watchConfiguration;
                this.fileMonitor.MonitorProject(project.ProjectFilePath, watchConfiguration.Glob);
            } 
        }

        public void ActOn(IEnumerable<string> files, Trigger trigger)
        {
            var filesToActOn = files.ToArray();
            var isFileUpdate = filesToActOn.Any(this.fileMonitor.IsMonitoredFile);
            var isProjectUpdate = filesToActOn.Any(this.fileMonitor.IsMonitoredProject);

            if (isFileUpdate || isProjectUpdate)
            {
                this.UpdateMonitors(filesToActOn, trigger);

                if (Trigger.Save.Equals(trigger) || Trigger.Delete.Equals(trigger))
                {
                    var actions = this.actionFactory.Create(this.configuration, Enumerable.Empty<string>());
                    this.taskDispatcher.QueueOnChangeTask(actions.FirstOrDefault());
                }
            }
        }

        private void UpdateMonitors(string[] files, Trigger trigger)
        {
            if (Trigger.Delete.Equals(trigger))
            {
                this.fileMonitor.UnMonitor(files);
            }

            if (Trigger.Save.Equals(trigger))
            {
                var monitoredProjects = files.Where(this.fileMonitor.IsMonitoredProject).ToArray();
                if (monitoredProjects.Any())
                {
                    this.fileMonitor.UnMonitor(monitoredProjects); 
                }
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

        public void Dispose()
        {
            this.fileMonitor.Dispose();
        }
    }
}
