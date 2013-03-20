namespace Goose.Core.Action
{
    using Goose.Core.Configuration;

    public interface IShellCommandBuilder
    {
        ShellCommand Build(ActionConfiguration configuration, CommandEvironmentVariables environmentVariables);
    }
}
