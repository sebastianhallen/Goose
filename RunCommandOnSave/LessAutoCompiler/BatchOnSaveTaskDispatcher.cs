namespace tretton37.RunCommandOnSave.LessAutoCompiler
{
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using OnSaveTask;

	public class BatchOnSaveTaskDispatcher
        : IOnSaveTaskDispatcher
    {
        private readonly IOnSaveActionTaskFactory taskFactory;        
        private readonly BlockingCollection<string> mainQueue = new BlockingCollection<string>();
        private readonly BlockingCollection<string> currentBuildQueue = new BlockingCollection<string>();  
        private readonly object syncLock = new object();
        private bool isBuilding;        

        public BatchOnSaveTaskDispatcher(IOnSaveActionTaskFactory taskFactory)
        {
            this.taskFactory = taskFactory;
            this.isBuilding = false;            
        }

        public void QueueOnChangeTaskFor(string filePath)
        {            
            if (!this.mainQueue.Contains(filePath))
            {                
                this.mainQueue.TryAdd(filePath);                    
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
                    var projectPath = "";
                    while (this.currentBuildQueue.TryTake(out projectPath))
                    {
                        var onSaveAction = this.taskFactory.CreateOnSaveAction(projectPath);
                        onSaveAction.Start();
                        buildTasks.Add(onSaveAction);

                    }

                    Task.WaitAll(buildTasks.ToArray());

                    return buildTasks.Any();
                }).ContinueWith(task =>
                    {
                        if (task.Result)
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
                    var projectPath = "";
                    while (this.mainQueue.TryTake(out projectPath))
                    {
                        this.currentBuildQueue.TryAdd(projectPath);
                    }
                }
            }                                   
        }
    }
}
