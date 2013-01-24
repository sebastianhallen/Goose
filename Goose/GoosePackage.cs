namespace Goose
{
    using System.Runtime.InteropServices;
    using Core;
    using Core.Dispatcher;
    using Core.OnSaveTask;
    using Core.Solution;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[Guid(GuidList.guidRunCommandOnSavePkgString)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
	public sealed class GoosePackage : Package
	{
		private LessFileOnSaveListener fileChangeListener;

		protected override void Dispose(bool disposing)
		{
			if (!disposing && this.fileChangeListener != null)
			{
				this.fileChangeListener.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void Initialize()
		{
			var fileChangeService = (IVsFileChangeEx)this.GetService(typeof(SVsFileChangeEx));

			var solutionFilesService = new SolutionFilesService(this);
			var outputService = new OutputService(this);
			var onSaveTaskFactory = new RunPowerShellCommandOnSaveTaskFactory(outputService);
			var onSaveTaskDispatcher = new BufferedOnSaveTaskDispatcher(onSaveTaskFactory);

			this.fileChangeListener = new LessFileOnSaveListener(fileChangeService, solutionFilesService, onSaveTaskDispatcher);

			base.Initialize();


		}

	}
}
