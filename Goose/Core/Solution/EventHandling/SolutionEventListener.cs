namespace Goose.Core.Solution.EventHandling
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Action;
    using Configuration;
    using Debugging;
    using Dispatcher;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Output;

    public class SolutionEventListener
        : IVsSolutionEvents, IDisposable
    {
        private readonly IVsFileChangeEx fileChangeService;
        private readonly IOutputService outputService;
        private readonly uint monitorCookie;
        private IVsSolution solution;

        private readonly LegacyFallbackActionConfigurationParser configParser;
        private readonly ISolutionFilesService solutionFilesService;
        private readonly IGlobMatcher globMatcher;
        private readonly IOnChangeTaskDispatcher onChangeTaskDispatcher;
        private readonly IGooseActionFactory actionFactory;
        private readonly ConcurrentDictionary<string, List<FileEventListener>> fileEventListeners;
        private volatile bool disposing = false;

        public SolutionEventListener(IVsSolution solution, IVsFileChangeEx fileChangeService, IOutputService outputService)
        {
            this.solution = solution;
            this.fileChangeService = fileChangeService;
            this.outputService = outputService;

            this.configParser = new LegacyFallbackActionConfigurationParser();
            this.solutionFilesService = new SolutionFilesService(this.solution, this.outputService);
            this.globMatcher = new RegexGlobMatcher();
            this.onChangeTaskDispatcher = new SynchronousOnChangeTaskDispatcher(this.outputService);            
            this.actionFactory = new GooseActionFactory(new PowerShellTaskFactory(this.outputService, new JsonCommandLogParser()));
            this.fileEventListeners = new ConcurrentDictionary<string, List<FileEventListener>>();

            this.monitorCookie = 0;
            if (this.solution != null)
            {
                this.solution.AdviseSolutionEvents(this, out this.monitorCookie);
            }
        }

        private void UnhookGoose(string projectPath)
        {
            List<FileEventListener> eventListeners;
            if (this.fileEventListeners.TryRemove(projectPath, out eventListeners))
            {
                this.outputService.Debug<SolutionEventListener>("disconnecting from " + projectPath);
                foreach (var eventListener in eventListeners)
                {
                    eventListener.Dispose();   
                }                
            }
        }

        private void HookupGoose(IVsProject projectHierarchy)
        {
            try
            {                
                this.ConnectProjectEventListeners(new SolutionProject(projectHierarchy, this.outputService));
            }
            catch (Exception ex)
            {
                //this.outputService.Handle(new CommandOutput("goose", "failed to configure actions", ex.ToString(), CommandOutputItemType.Message));
            }
        }

        private void ConnectProjectEventListeners(ISolutionProject project)
        {
            var projectRoot = Path.GetDirectoryName(project.ProjectFilePath);
            var configPath = Path.Combine(projectRoot, "goose.config");
            if (!File.Exists(configPath)) return;


            this.outputService.Debug<SolutionEventListener>("Connecting to: " + project.ProjectFilePath);
            var actionConfigurations = this.configParser.Parse(projectRoot, File.OpenRead(configPath));
            foreach (var action in actionConfigurations)
            {
                var gooseAction = action;                
                this.fileEventListeners.AddOrUpdate(
                    project.ProjectFilePath,
                    new List<FileEventListener>{ this.CreateFileEventListener(project, gooseAction) },
                    (key, existingListener) =>
                    {
                        existingListener.Add(this.CreateFileEventListener(project, gooseAction));
                        return existingListener;
                    });
            }
        }

        private FileEventListener CreateFileEventListener(ISolutionProject project, ActionConfiguration action)
        {
            var fileChangeSubscriber = new FileChangeSubscriber(this.fileChangeService);
            var fileMonitor = new FileMonitor(this.solutionFilesService, this.globMatcher, fileChangeSubscriber, this.outputService);                

            var eventListener = new FileEventListener(fileMonitor, this.onChangeTaskDispatcher, this.actionFactory, fileChangeSubscriber);
            eventListener.Initialize(project, action);
            return eventListener;
        }


        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            var projectHierarchy = pHierarchy as IVsProject;
            if (projectHierarchy != null)
            {
                this.HookupGoose(projectHierarchy);
            }
                      

            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            var project = pHierarchy.AsProject();

            if (project != null)
            {
                this.UnhookGoose(project.FileName);
            }
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {            
            return VSConstants.S_OK;
        }        

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public void Dispose()
        {
            if (!this.disposing)
            {
                this.disposing = true;
                foreach (var monitoredProject in this.fileEventListeners.Keys.ToArray())
                {
                    List<FileEventListener> listeners;
                    if (this.fileEventListeners.TryRemove(monitoredProject, out listeners))
                    {
                        foreach (var listener in listeners)
                        {
                            listener.Dispose();   
                        }                        
                    }
                }
                if (this.monitorCookie != 0)
                {
                    this.solution.UnadviseSolutionEvents(this.monitorCookie);
                }
                this.solution = null;
            }
        }
    }
}
