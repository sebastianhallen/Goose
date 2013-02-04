namespace Goose.Core.Dispatcher
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Action;
    using Output;

    public abstract class OnChangeTaskDispatcher
        : IOnChangeTaskDispatcher
    {
        private readonly IOutputService outputService;
        protected readonly BlockingCollection<IGooseAction> mainQueue = new BlockingCollection<IGooseAction>();
        protected readonly BlockingCollection<IGooseAction> currentBuildQueue = new BlockingCollection<IGooseAction>();
        protected readonly object syncLock = new object();
        protected bool isBuilding;

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
            Task.Factory.StartNew(() =>
            {
                this.CreateBuildQueue();

                this.StartOnSaveActions();
            });
        }

        private void StartOnSaveActions()
        {
            Task.Factory.StartNew(() =>
            {
                var buildTasks = new List<Task>();
                IGooseAction workItem;
                while (this.currentBuildQueue.TryTake(out workItem))
                {
                    var work = workItem.Work;
                    this.outputService.Handle(new CommandOutput("goose", string.Format("running action: {0}", workItem.StartMessage), "", CommandOutputItemType.Message));
                    work.Start();
                    buildTasks.Add(work);
                }

                Task.WaitAll(buildTasks.ToArray());

                return buildTasks.Any();
            }).ContinueWith(task =>
            {
                if (task.Exception != null || task.Result)
                {
                    lock (this.syncLock)
                    {
                        this.isBuilding = false;
                    }
                    if (this.mainQueue.Any())
                    {
                        this.TriggerBuild();
                    }
                }               
            });
        }     
    }
}