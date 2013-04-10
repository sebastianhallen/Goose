namespace Goose.Core.Solution
{
    using Action;
    using Configuration;
    using Dispatcher;
    using EventHandling;
    using Goose.Core.Action.PowerShell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Output;

    public interface IFileEventListenerFactory
    {
        FileEventListener Create(ISolutionProject project, ActionConfiguration actionConfiguration);
    }

    public class DefaultFileEventListenerFactory
        : IFileEventListenerFactory
    {        
        private readonly IVsFileChangeEx fileChangeService;
        private readonly IOutputService outputService;

        private readonly ISolutionFilesService solutionFilesService;
        private readonly IGlobMatcher globMatcher;
        private readonly IOnChangeTaskDispatcher onChangeTaskDispatcher;
        private readonly IGooseActionFactory actionFactory;

        public DefaultFileEventListenerFactory(ISolutionFilesService solutionFilesService, IVsFileChangeEx fileChangeService, IOutputService outputService, ICommandErrorReporter errorReporter)
        {
            this.solutionFilesService = solutionFilesService;
            this.fileChangeService = fileChangeService;
            this.outputService = outputService;
            
            this.globMatcher = new RegexGlobMatcher();
            this.onChangeTaskDispatcher = new SynchronousOnChangeTaskDispatcher(this.outputService);
            this.actionFactory = new PowerShellGooseActionFactory(new PowerShellTaskFactory(this.outputService, errorReporter, new JsonCommandLogParser()), new PowerShellCommandBuilder());
        }

        public FileEventListener Create(ISolutionProject project, ActionConfiguration actionConfiguration)
        {
            var fileChangeSubscriber = new FileChangeSubscriber(this.fileChangeService);
            var fileMonitor = new FileMonitor(this.solutionFilesService, this.globMatcher, fileChangeSubscriber, this.outputService);

            var eventListener = new FileEventListener(fileMonitor, this.onChangeTaskDispatcher, this.actionFactory, fileChangeSubscriber);
            eventListener.Initialize(project, actionConfiguration);
            return eventListener;
        }
    }
    

}