namespace Goose.Core.Action.PowerShell
{
    public class PowerShellCommandBuilder
        : IShellCommandBuilder
    {
        public ShellCommand Build(string workingDirectory, string command, CommandEvironmentVariables environmentVariables)
        {
            var payload = command.Replace("{file-path}", environmentVariables.FilePath);
            
            return new ShellCommand(workingDirectory, payload);

        }
    }
}