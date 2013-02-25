namespace Goose.Core.Solution
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Action;
    using Configuration;
    using Dispatcher;
    using EventHandling;
    using Microsoft.VisualStudio.Shell.Interop;
    using Output;

    public interface IFileEventListenerFactory
    {
        FileEventListener Create(ISolutionProject project, ActionConfiguration actionConfiguration);
    }

    public class DefaultFileEventListenerFactory
        : IFileEventListenerFactory
    {
        private readonly IVsSolution solution;
        private readonly IVsFileChangeEx fileChangeService;
        private readonly IOutputService outputService;

        private readonly ISolutionFilesService solutionFilesService;
        private readonly IGlobMatcher globMatcher;
        private readonly IOnChangeTaskDispatcher onChangeTaskDispatcher;
        private readonly IGooseActionFactory actionFactory;

        public DefaultFileEventListenerFactory(IVsSolution solution, IVsFileChangeEx fileChangeService, IOutputService outputService)
        {
            this.solution = solution;
            this.fileChangeService = fileChangeService;
            this.outputService = outputService;

            this.solutionFilesService = new SolutionFilesService(this.solution, this.outputService);
            this.globMatcher = new RegexGlobMatcher();
            this.onChangeTaskDispatcher = new SynchronousOnChangeTaskDispatcher(this.outputService);
            this.actionFactory = new GooseActionFactory(new PowerShellTaskFactory(this.outputService, new JsonCommandLogParser()));
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