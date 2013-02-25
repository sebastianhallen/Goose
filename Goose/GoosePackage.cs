namespace Goose
{
    using System;
    using System.Runtime.InteropServices;
    using Core.Output;
    using Core.Solution.EventHandling;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.4.6", IconResourceID = 400)]
	[Guid(GuidList.guidGoosePkgString)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string)]
	public sealed class GoosePackage
        : Package
    {
        private IVsFileChangeEx fileChangeService;
        private IOutputService outputService;
        private SolutionEventListener solutionEventListener;

        protected override void Dispose(bool disposing)
        {
            this.DisposeGooseListerners(disposing);
            base.Dispose(disposing);
        }

        private void DisposeGooseListerners(bool disposing)
        {
            if (!disposing)
            {
                this.solutionEventListener.Dispose();
            }            
        }

        protected override void Initialize()
		{
            this.outputService = new OutputService(this);
			this.fileChangeService = (IVsFileChangeEx)this.GetService(typeof(SVsFileChangeEx));

            var solution = (IVsSolution) this.GetService(typeof (SVsSolution));

            this.solutionEventListener = new SolutionEventListener(solution, this.fileChangeService, this.outputService);
            base.Initialize();
		}
	}
}
