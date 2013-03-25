namespace Goose.Core.Solution.EventHandling
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Debugging;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Output;
    using WatchItem = System.Tuple<FileEventListener, Configuration.ActionConfiguration>;

    public class SolutionEventListener
        : IVsSolutionEvents, IDisposable
    {
        private readonly IOutputService outputService;
        private readonly uint monitorCookie;
        private IVsSolution solution;
        private readonly IFileEventListenerFactory fileEventListenerFactory;

        private readonly IConfigReader configReader = new DefaultConfigReader();  
        private readonly ConcurrentDictionary<string, List<WatchItem>> fileEventListeners;
        private volatile bool disposing = false;

        public SolutionEventListener(IVsSolution solution, IFileEventListenerFactory fileEventListenerFactory, IOutputService outputService)
        {
            this.solution = solution;
            this.fileEventListenerFactory = fileEventListenerFactory;            
            this.outputService = outputService;


            this.fileEventListeners = new ConcurrentDictionary<string, List<WatchItem>>();

            this.monitorCookie = 0;
            if (this.solution != null)
            {
                this.solution.AdviseSolutionEvents(this, out this.monitorCookie);
            }
        }

        private void UnhookGoose(string projectPath)
        {
            List<WatchItem> eventListeners;
            if (this.fileEventListeners.TryRemove(projectPath, out eventListeners))
            {
                this.outputService.Debug<SolutionEventListener>("disconnecting from " + projectPath);
                foreach (var eventListener in eventListeners)
                {
                    eventListener.Item1.Dispose();   
                }
            }            
        }

        private void HookupGoose(IVsProject projectHierarchy)
        {
            try
            {
                var solutionProject = new SolutionProject(this.solution, projectHierarchy, this.outputService);
                this.ConnectProjectEventListeners(solutionProject);
            }
            catch (Exception ex)
            {
                //this.outputService.Handle(new CommandOutput("goose", "failed to configure actions", ex.ToString(), CommandOutputItemType.Message));
            }
        }

        private void ConnectProjectEventListeners(ISolutionProject project)
        {
            var actionConfigurations = this.configReader.GetActionConfigurations(project);
                            
            foreach (var action in actionConfigurations)
            {
                this.outputService.Debug<SolutionEventListener>("Connecting to: " + project.ProjectFilePath);
                var actionConfiguration = action;
                this.fileEventListeners.AddOrUpdate(
                    project.ProjectFilePath,
                    new List<WatchItem>
                    {
                        new WatchItem(this.fileEventListenerFactory.Create(project, actionConfiguration), actionConfiguration)
                    },
                    (key, existingListener) =>
                    {
                        if (!existingListener.Any(watchItem => watchItem.Item2.Equals(actionConfiguration)))
                        {
                            var listener = this.fileEventListenerFactory.Create(project, actionConfiguration);
                            existingListener.Add(new WatchItem(listener, actionConfiguration));
                        }
                        return existingListener;
                    });
            }            
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
            this.outputService.RemovePanels();
            return VSConstants.S_OK;
        }

        public void Dispose()
        {
            if (!this.disposing)
            {
                this.disposing = true;
                var monitoredProjects = this.fileEventListeners.Keys.ToArray();
                foreach (var project in monitoredProjects)
                {                  
                    this.UnhookGoose(project);
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
