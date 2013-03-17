namespace Goose.Core.Action
{
    public interface IShellCommandBuilder
    {
        ShellCommand Build(string workingDirectory, string command, CommandEvironmentVariables evironmentVariables);
    }


    public class CommandEvironmentVariables
    {

    }
}
