namespace Goose
{
    using System.IO;
    using System.Runtime.InteropServices;
    using Core;
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
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
	public sealed class GoosePackage : Package
	{
        private FileEventListener fileEventListener;

		protected override void Dispose(bool disposing)
		{
            //if (!disposing && this.fileEventListener != null)
            //{
            //    this.fileChangeListener.Dispose();
            //}
			base.Dispose(disposing);
		}

		protected override void Initialize()
		{
			var fileChangeService = (IVsFileChangeEx)this.GetService(typeof(SVsFileChangeEx));

			var solutionFilesService = new SolutionFilesService(this);
		    var fileChangeSubscriber = new FileChangeSubscriber(fileChangeService);
		    var globMatcher = new RegexGlobMatcher();
            var fileMonitor = new FileMonitor(solutionFilesService, globMatcher, fileChangeSubscriber);
			var onChangeTaskDispatcher = new BufferedOnChangeTaskDispatcher();

            var outputService = new OutputService(this);
		    var logParser = new JsonCommandLogParser();
		    var powerShellTaskFactory = new PowerShellTaskFactory(outputService, logParser);
		    var actionFactory = new GooseActionFactory(powerShellTaskFactory);

            this.fileEventListener = new FileEventListener(solutionFilesService, fileMonitor, onChangeTaskDispatcher, actionFactory, fileChangeSubscriber);

		    var configParser = new LegacyFallbackActionConfigurationParser();
		    foreach (var project in solutionFilesService.Projects)
		    {
		        var projectRoot = Path.GetDirectoryName(project.ProjectFilePath);
		        var configPath = Path.Combine(projectRoot, "goose.config");
		        if (File.Exists(configPath))
		        {
		            var config = configParser.Parse(projectRoot, File.OpenRead(configPath));

                    this.fileEventListener.Initialize(config);
                }
		    }
			
            base.Initialize();


		}

	}
}
