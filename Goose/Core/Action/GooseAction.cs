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
                var powerShellCommand = string.Format(@"cd ""{0}"" ; {1}", workingDirectoryAbsolutePath, this.command);
                return this.powershellTaskFactory.Create(powerShellCommand);
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

    public class VoidGooseAction
        : IGooseAction
    {
        public Task Work { get { return new Task(() => { });} }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}