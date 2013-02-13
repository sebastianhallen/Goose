namespace Goose.Core.Dispatcher
{
    using System.Collections.Concurrent;
    using System.Linq;
    using Action;
    using Output;

    public abstract class OnChangeTaskDispatcher
        : IOnChangeTaskDispatcher
    {
        private readonly IOutputService outputService;
        protected readonly BlockingCollection<IGooseAction> mainQueue = new BlockingCollection<IGooseAction>();
        protected readonly BlockingCollection<IGooseAction> currentBuildQueue = new BlockingCollection<IGooseAction>();
        protected volatile bool isBuilding;

        protected OnChangeTaskDispatcher(IOutputService outputService)
        {
            this.outputService = outputService;
            this.isBuilding = false;
        }

        public void QueueOnChangeTask(IGooseAction action)
        {
            if (!this.mainQueue.Contains(action) && this.mainQueue.TryAdd(action))
            {
                this.TriggerBuild();
            }
        }

        protected abstract void CreateBuildQueue();

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