namespace Goose.Core.Output
{
    using System.Collections.Generic;
    using System.Linq;
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using Microsoft.VisualStudio.Shell;

    public class CommandErrorReporter : ICommandErrorReporter
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
            this.errorTaskHandler.Remove(command);
            
            var resultTasks = this.BuildErrorTask(command, result.Result);            
            this.errorTaskHandler.Add(resultTasks);            
        }

        private IEnumerable<IGooseErrorTask> BuildErrorTask(ShellCommand command, string content)
        {
            var output = this.logParser.Parse(content);

            return output.Results
                         .Where(error => error.Type.Equals(CommandOutputItemType.Error))
                         .Select(error =>
                             {
                                 var errorTask = new ErrorTask
                                     {
                                         CanDelete = true,
                                         Column = 0,
                                         Line = (int) error.Line,
                                         Document = error.FullPath,
                                         Text = error.Message
                                     };
                                 return new GooseErrorTask(command, errorTask);
                             });                             
        }
    }
}
