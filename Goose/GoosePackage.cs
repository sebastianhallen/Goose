namespace Goose
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;    
    using Core.Action;
    using Core.Configuration;
    using Core.Dispatcher;
    using Core.Output;
    using Core.Solution;
    using Core.Solution.EventHandling;
    using Debugging;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.4.4", IconResourceID = 400)]
	[Guid(GuidList.guidGoosePkgString)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
	public sealed class GoosePackage : Package
	{
        private IList<FileEventListener> fileEventListeners;
        private IOnChangeTaskDispatcher onChangeTaskDispatcher;
        private IGlobMatcher globMatcher;
        private ISolutionFilesService solutionFilesService;
        private IVsFileChangeEx fileChangeService;
        private IOutputService outputService;
        private ICommandLogParser logParser;
        private IPowerShellTaskFactory powerShellTaskFactory;
        private IGooseActionFactory actionFactory;
        private LegacyFallbackActionConfigurationParser configParser;

        protected override void Dispose(bool disposing)
		{
            if (!disposing)
            {
                foreach (var listener in this.fileEventListeners)
                {
                    listener.Dispose();   
                }                
            }
			base.Dispose(disposing);
		}

		protected override void Initialize()
		{
            this.outputService = new OutputService(this);
			this.fileChangeService = (IVsFileChangeEx)this.GetService(typeof(SVsFileChangeEx));
            this.solutionFilesService = new SolutionFilesService(this, this.outputService);
		    this.globMatcher = new RegexGlobMatcher();
            
			//this.onChangeTaskDispatcher = new BufferedOnChangeTaskDispatcher();
            this.onChangeTaskDispatcher = new SynchronousOnChangeTaskDispatcher(this.outputService);            
		    this.logParser = new JsonCommandLogParser();
		    this.powerShellTaskFactory = new PowerShellTaskFactory(this.outputService, this.logParser);
		    this.actionFactory = new GooseActionFactory(this.powerShellTaskFactory);
            this.configParser = new LegacyFallbackActionConfigurationParser();		    
            this.fileEventListeners = new List<FileEventListener>();            

		    foreach (var project in this.solutionFilesService.Projects)
		    {
                var projectPath = project.ProjectFilePath;
                if (string.IsNullOrWhiteSpace(projectPath))
                {                    
                    this.outputService.Handle(new CommandOutput("goose", "project path was null, skipping", "", CommandOutputItemType.Message));
                    this.outputService.Debug<GoosePackage>("project path null in Initialize");
                    continue;
                }

		        var projectRoot = "";
                var configPath = "";
		        try
		        {
		            projectRoot = Path.GetDirectoryName(projectPath);
		            configPath = Path.Combine(projectRoot, "goose.config");
		            if (File.Exists(configPath))
		            {
		                this.ConnectSolutionEventListeners(projectRoot, configPath, project);
		            }
		        }
		        catch (Exception ex)
		        {
                    this.outputService.Handle(new CommandOutput("goose", "failed to configure actions", ex.ToString(), CommandOutputItemType.Message));                    
		        }               		        
		    }
			
            base.Initialize();
		}

        private void ConnectSolutionEventListeners(string projectRoot, string configPath, ISolutionProject project)
        {
            var actionConfigurations = this.configParser.Parse(projectRoot, File.OpenRead(configPath));
            foreach (var action in actionConfigurations)
            {
                var fileChangeSubscriber = new FileChangeSubscriber(this.fileChangeService);
                var fileMonitor = new FileMonitor(this.solutionFilesService, this.globMatcher, fileChangeSubscriber, this.outputService);
                var eventListener = new FileEventListener(fileMonitor, this.onChangeTaskDispatcher, this.actionFactory, fileChangeSubscriber);
                eventListener.Initialize(project, action);
                this.fileEventListeners.Add(eventListener);
            }
        }
	}
}
