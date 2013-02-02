namespace Goose.Core.Solution.EventHandling
{
    using Microsoft.VisualStudio.Shell.Interop;

    public class FileChangeSubscriber
        : IFileChangeSubscriber
    {
        private const uint FileChangeFlags = (uint)_VSFILECHANGEFLAGS.VSFILECHG_Add | (uint)_VSFILECHANGEFLAGS.VSFILECHG_Del | (uint)_VSFILECHANGEFLAGS.VSFILECHG_Size | (uint)_VSFILECHANGEFLAGS.VSFILECHG_Time;
        private readonly IVsFileChangeEx fileChangeEx;
        private IFileChangeConsumer fileChangeConsumer;

        public FileChangeSubscriber(IVsFileChangeEx fileChangeEx)
        {
            this.fileChangeEx = fileChangeEx;
        }

        public MonitoredFile Watch(string file)
        {
            uint cookie;
            this.fileChangeEx.AdviseFileChange(file, FileChangeFlags, this.fileChangeConsumer, out cookie);

            return new MonitoredFile(cookie, file);
        }

        public void Attach(IFileChangeConsumer fileChangeConsumer)
        {
            this.fileChangeConsumer = fileChangeConsumer;
        }
    }
}