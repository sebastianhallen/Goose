namespace Goose.Core.Action
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IGooseAction
    {
        Task Work { get; }
    }

    public class PowerShellGooseAction
        : IGooseAction
    {
        private readonly IPowerShellTaskFactory powershellTaskFactory;
        private readonly string rootPath;
        private readonly string workingDirectory;
        private readonly string command;

        public PowerShellGooseAction(IPowerShellTaskFactory powershellTaskFactory, string rootPath, string workingDirectory, string command)
        {
            this.powershellTaskFactory = powershellTaskFactory;
            this.rootPath = rootPath;
            this.workingDirectory = workingDirectory;
            this.command = command;                        
        }

        public Task Work 
        {
            get
            {
                var workingDirectoryAbsolutePath = Path.Combine(this.rootPath, this.workingDirectory);
                var command = string.Format(@"cd ""{0}"" ; {1}", workingDirectoryAbsolutePath, this.command);
                return this.powershellTaskFactory.Create(command);
            }
        }
    }

    public class VoidGooseAction
        : IGooseAction
    {
        public Task Work { get { return new Task(() => { });} }
    }
}