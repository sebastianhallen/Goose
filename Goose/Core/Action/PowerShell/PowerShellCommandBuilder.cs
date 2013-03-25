namespace Goose.Core.Action.PowerShell
{
    using System.IO;
    using Goose.Core.Configuration;

    public class PowerShellCommandBuilder
        : IShellCommandBuilder
    {
        public ShellCommand Build(ActionConfiguration configuration, CommandEvironmentVariables environmentVariables)
        {
            var solutionRoot = configuration.SolutionRoot;
            var projectRoot = configuration.ProjectRoot;
            var relativeWorkingDirectory = configuration.RelativeWorkingDirectory;
            var absoluteWorkingDirectory = Path.Combine(projectRoot, relativeWorkingDirectory);
            var absoluteFilePath = environmentVariables.FilePath;
            var relativeFilePath = string.IsNullOrEmpty(projectRoot)
                                       ? absoluteFilePath
                                       : absoluteFilePath.Replace(projectRoot, "").TrimStart('\\', '/');

            
            var payload = configuration.Command
                                       .Replace("{absolute-file-path}", absoluteFilePath)
                                       .Replace("{relative-file-path}", relativeFilePath)
                                       .Replace("{solution-root}", solutionRoot)
                                       .Replace("{project-root}", projectRoot)
                                       .Replace("{working-directory}", absoluteWorkingDirectory);

            return new ShellCommand(absoluteWorkingDirectory, payload);
        }
    }
}