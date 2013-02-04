namespace Goose
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;    
    using Core.Action;
    using Core.Configuration;
    using Core.Dispatcher;
    using Core.Output;
    using Core.Solution;
    using Core.Solution.EventHandling;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[Guid(GuidList.guidRunCommandOnSavePkgString)]
	//[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
	public sealed class GoosePackage : Package
	{
        private IList<FileEventListener> fileEventListeners;

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
			var fileChangeService = (IVsFileChangeEx)this.GetService(typeof(SVsFileChangeEx));

			var solutionFilesService = new SolutionFilesService(this);
		    var globMatcher = new RegexGlobMatcher();            
			var onChangeTaskDispatcher = new BufferedOnChangeTaskDispatcher();

            var outputService = new OutputService(this);
		    var logParser = new JsonCommandLogParser();
		    var powerShellTaskFactory = new PowerShellTaskFactory(outputService, logParser);
		    var actionFactory = new GooseActionFactory(powerShellTaskFactory);

            this.fileEventListeners = new List<FileEventListener>();            

		    var configParser = new LegacyFallbackActionConfigurationParser();
		    foreach (var project in solutionFilesService.Projects)
		    {
                var projectPath = project.ProjectFilePath;
                if (string.IsNullOrWhiteSpace(projectPath))
                {
                    outputService.Handle(new CommandOutput("goose", "project path was null, skipping", "", CommandOutputItemType.Message));
                    continue;
                }
                var projectRoot = Path.GetDirectoryName(projectPath);
		        var configPath = Path.Combine(projectRoot, "goose.config");
		        if (File.Exists(configPath))
		        {
                    var fileChangeSubscriber = new FileChangeSubscriber(fileChangeService);
                    var fileMonitor = new FileMonitor(solutionFilesService, globMatcher, fileChangeSubscriber);

		            var config = configParser.Parse(projectRoot, File.OpenRead(configPath));
                    var eventListener = new FileEventListener(fileMonitor, onChangeTaskDispatcher, actionFactory, fileChangeSubscriber);
                    eventListener.Initialize(project, config);
                    this.fileEventListeners.Add(eventListener);                    
                }
		    }
			
            base.Initialize();
		}

	}
}
