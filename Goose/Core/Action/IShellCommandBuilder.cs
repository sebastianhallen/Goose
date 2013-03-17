namespace Goose.Core.Action
{
    public interface IShellCommandBuilder
    {
        ShellCommand Build(string workingDirectory, string command, CommandEvironmentVariables environmentVariables);
    }


    public class CommandEvironmentVariables
    {
        public string FilePath { get; set; }
    }
}
