namespace Goose.Core.Output
{
    using System.Collections.Generic;
    using System.Linq;
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using Microsoft.VisualStudio.Shell;

    public class CommandErrorReporter
    {
        private readonly IErrorTaskHandler errorTaskHandler;
        private readonly ICommandLogParser logParser;

        public CommandErrorReporter(IErrorTaskHandler errorTaskHandler, ICommandLogParser logParser)
        {
            this.errorTaskHandler = errorTaskHandler;
            this.logParser = logParser;
        }

        public void Report(ShellCommand command, CommandResult result)
        {
            var resultTasks = this.BuildErrorTask(command, result.Result);

            foreach (var errorTask in resultTasks)
            {
                this.errorTaskHandler.Add(errorTask);
            }
        }

        private IEnumerable<GooseErrorTask> BuildErrorTask(ShellCommand command, string content)
        {
            var output = this.logParser.Parse(content);

            return output.Results
                         .Where(error => error.Type.Equals(CommandOutputItemType.Error))
                         .Select(error => new GooseErrorTask(command, error));
        }
    }

    public interface IErrorTaskHandler
    {
        void Add(GooseErrorTask task);
    }

    public class GooseErrorTask
        : ErrorTask
    {
        private readonly ShellCommand command;
        private readonly CommandOutputItem error;

        public GooseErrorTask(ShellCommand command, CommandOutputItem error)
        {
            this.command = command;
            this.error = error;
        }
    }
}
