namespace Goose.Core.Dispatcher
{
    using Action;
    using Output;

    public class SynchronousOnChangeTaskDispatcher
        : OnChangeTaskDispatcher
    {
        public SynchronousOnChangeTaskDispatcher(IOutputService outputService)
            : base(outputService)
        {
            
        }

        protected override void CreateBuildQueue()
        {
            if (!this.isBuilding)
            {
                this.isBuilding = true;
                IGooseAction workItem;
                if (this.mainQueue.TryTake(out workItem))
                {
                    this.currentBuildQueue.TryAdd(workItem);
                }
            }            
        }
    }
}