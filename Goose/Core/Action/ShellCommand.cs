namespace Goose.Core.Action
{
    public class ShellCommand
    {
        public string WorkingDirectory { get; private set; }
        public string Command { get; private set; }

        public ShellCommand(string workingDirectory, string command)
        {
            this.WorkingDirectory = workingDirectory;
            this.Command = command;
        }

        public override string ToString()
        {

            return string.Format("{0}> {1}", this.WorkingDirectory, this.Command);
        }

        protected bool Equals(ShellCommand other)
        {
            return string.Equals(WorkingDirectory, other.WorkingDirectory) && string.Equals(Command, other.Command);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ShellCommand) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((WorkingDirectory != null ? WorkingDirectory.GetHashCode() : 0)*397) ^ (Command != null ? Command.GetHashCode() : 0);
            }
        }
    }
}