namespace Goose.Core.Action
{
    using System.IO;
    using System.Threading.Tasks;
    using Goose.Core.Configuration;

    public interface IGooseAction
    {
        string StartMessage { get; }
        Task Work { get; }
    }

    public class PowerShellGooseAction
        : IGooseAction
    {
        private readonly IPowerShellTaskFactory powershellTaskFactory;
        private readonly string rootPath;
        private readonly string filePath;
        private readonly string workingDirectory;
        private readonly string command;
        private readonly CommandScope scope;
        private readonly object taskCreationLock = new object();

        public PowerShellGooseAction(IPowerShellTaskFactory powershellTaskFactory, string rootPath, string filePath, string workingDirectory, string command, CommandScope scope)
        {
            this.powershellTaskFactory = powershellTaskFactory;
            this.rootPath = rootPath;
            this.filePath = filePath;
            this.workingDirectory = workingDirectory;
            this.command = command;
            this.scope = scope;
        }

        private string Command
        {
            get
            {
                var workingDirectoryAbsolutePath = Path.Combine(this.rootPath, this.workingDirectory);
                var powerShellCommand = string.Format(@"cd ""{0}"" ; {1}", workingDirectoryAbsolutePath, this.command);
                return powerShellCommand;
            }
        }

        public string StartMessage
        {
            get { return this.Command; }
        }

        private Task workField;
        public Task Work 
        {
            get
            {
                lock (this.taskCreationLock)
                {
                    if (this.workField == null || !TaskStatus.Created.Equals(this.workField.Status))
                    {
                        this.workField = this.powershellTaskFactory.Create(this.Command);
                    }
                    return this.workField;   
                }
            }
        }

        protected bool Equals(PowerShellGooseAction other)
        {
            return string.Equals(rootPath, other.rootPath) && 
                   string.Equals(workingDirectory, other.workingDirectory) && 
                   string.Equals(command, other.command);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PowerShellGooseAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (rootPath != null ? rootPath.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (workingDirectory != null ? workingDirectory.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (command != null ? command.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}