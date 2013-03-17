namespace Goose.Core.Action.PowerShell
{
    public class PowerShellCommandBuilder
        : IShellCommandBuilder
    {
        public ShellCommand Build(string workingDirectory, string command, CommandEvironmentVariables evironmentVariables)
        {
            return new ShellCommand(workingDirectory, command);
        }
    }
}