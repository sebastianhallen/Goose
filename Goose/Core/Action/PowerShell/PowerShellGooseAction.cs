namespace Goose.Core.Action.PowerShell
{
    using System.Threading.Tasks;

    public class PowerShellGooseAction
        : IGooseAction
    {
        private readonly IPowerShellTaskFactory powershellTaskFactory;
        private readonly ShellCommand command;
        private readonly object taskCreationLock = new object();

        public PowerShellGooseAction(IPowerShellTaskFactory powershellTaskFactory, ShellCommand command)
        {
            this.powershellTaskFactory = powershellTaskFactory;
            this.command = command;
        }

        public string StartMessage
        {
            get { return "running command: " + this.command; }
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
                        this.workField = this.powershellTaskFactory.Create(this.command);
                    }
                    return this.workField;
                }
            }
        }

        protected bool Equals(PowerShellGooseAction other)
        {
            return Equals(command, other.command);
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
            return (command != null ? command.GetHashCode() : 0);
        }
    }
}