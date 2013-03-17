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
    }
}