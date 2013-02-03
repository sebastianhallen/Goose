namespace Goose.Core.Dispatcher
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Action;

    public class BufferedOnChangeTaskDispatcher
		: IOnChangeTaskDispatcher
	{
		private readonly BlockingCollection<IGooseAction> mainQueue = new BlockingCollection<IGooseAction>();
        private readonly BlockingCollection<IGooseAction> currentBuildQueue = new BlockingCollection<IGooseAction>();
		private readonly object syncLock = new object();
		private bool isBuilding;

		public BufferedOnChangeTaskDispatcher()
		{
			this.isBuilding = false;
		}

        public void QueueOnChangeTask(IGooseAction action)
        {
            if (!this.mainQueue.Contains(action))
            {
                this.mainQueue.TryAdd(action);
            }

            TriggerBuild();
        }

        private void TriggerBuild()
		{
			Task.Factory.StartNew(() =>
				{
					System.Threading.Thread.Sleep(1000);
					this.CreateBuildQueue();

					this.StartOnSaveActions();

				}).ContinueWith(task =>
					{
						if (this.mainQueue.Any())
						{
							TriggerBuild();
						}
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
						workItem.Work.Start();
                        buildTasks.Add(workItem.Work);

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
						}
					});
		}

		private void CreateBuildQueue()
		{
			lock (this.syncLock)
			{
				if (!this.isBuilding)
				{
					this.isBuilding = true;
                    IGooseAction workItem;
					while (this.mainQueue.TryTake(out workItem))
					{
						this.currentBuildQueue.TryAdd(workItem);
					}
				}
			}
		}
	}
}
