namespace Goose.Core.Dispatcher
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Action;
    using Debugging;
    using Output;

    public class SynchronousOnChangeTaskDispatcher
        : IOnChangeTaskDispatcher
    {
        private readonly IOutputService outputService;
        private readonly BlockingCollection<IGooseAction> mainQueue = new BlockingCollection<IGooseAction>();
        private readonly BlockingCollection<IGooseAction> currentBuildQueue = new BlockingCollection<IGooseAction>();
        private volatile bool isBuilding;
        private volatile int currentJob;

        public SynchronousOnChangeTaskDispatcher(IOutputService outputService)
        {
            this.outputService = outputService;
            this.isBuilding = false;
        }

        public void QueueOnChangeTask(IGooseAction action)
        {
            if (!this.mainQueue.Contains(action) && 
                !this.currentBuildQueue.Contains(action) && 
                this.AllowedToPlaceInMainQueue(action) &&
                this.mainQueue.TryAdd(action))
            {
                this.outputService.Debug<SynchronousOnChangeTaskDispatcher>("adding to queue: " + action.StartMessage);
                this.TriggerBuild();
            }
        }

        private bool AllowedToPlaceInMainQueue(IGooseAction action)
        {
            return this.currentJob != action.GetHashCode();
        }

        private void CreateBuildQueue()
        {
            if (!this.isBuilding)
            {
                this.isBuilding = true;
                IGooseAction workItem;
                if (this.mainQueue.TryTake(out workItem))
                {
                    this.currentJob = workItem.GetHashCode();
                    this.currentBuildQueue.TryAdd(workItem);
                }
            }
        }
        private void TriggerBuild()
        {
            this.CreateBuildQueue();

            this.ActOnChange();
        }

        private void ActOnChange()
        {
            IGooseAction workItem;
            while (this.currentBuildQueue.TryTake(out workItem))
            {
                var work = workItem.Work;
                var nextItem = work.ContinueWith(task =>
                {
                    this.currentJob = 0;
                    this.isBuilding = false;
                    
                    if (this.mainQueue.Any())
                    {
                        this.TriggerBuild();
                    }
                });

                this.outputService.Handle(new CommandOutput("goose", string.Format("running action: {0}", workItem.StartMessage), "", CommandOutputItemType.Message));
                work.Start();
            }
        }     
    }
}