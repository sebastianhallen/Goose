namespace Goose.Core.Solution.EventHandling
{
    using Microsoft.VisualStudio.Shell.Interop;

    public class FileChangeSubscriber
        : IFileChangeSubscriber
    {
        private readonly IVsFileChangeEx fileChangeEx;

        public FileChangeSubscriber(IVsFileChangeEx fileChangeEx)
        {
            this.fileChangeEx = fileChangeEx;
        }

        public MonitoredFile Watch(string file)
        {
            throw new System.NotImplementedException();
        }

        public void Attach(IFileChangeConsumer fileMonitor)
        {
            throw new System.NotImplementedException();
        }
    }
}