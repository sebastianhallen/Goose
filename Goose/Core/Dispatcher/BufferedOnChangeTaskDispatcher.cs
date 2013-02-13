//namespace Goose.Core.Dispatcher
//{
//    using Action;
//    using Output;

//    public class BufferedOnChangeTaskDispatcher
//        : OnChangeTaskDispatcher
//    {
//        public BufferedOnChangeTaskDispatcher(IOutputService outputService) 
//            : base(outputService)
//        {
//        }

//        protected override void CreateBuildQueue()
//        {
//            lock (this.syncLock)
//            {
//                if (!this.isBuilding)
//                {
//                    this.isBuilding = true;
//                    IGooseAction workItem;
//                    while (this.mainQueue.TryTake(out workItem))
//                    {
//                        this.currentBuildQueue.TryAdd(workItem);
//                    }
//                }
//            }
//        }
//    }
//}
