namespace Goose.Core.Action.PowerShell
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Goose.Core.Output;

    public class PowerShellTaskFactory
        : IPowerShellTaskFactory
    {
        private readonly IOutputService outputService;
        private readonly ICommandErrorReporter errorReporter;
        private readonly ICommandLogParser logParser;
        private readonly IShellCommandRunner commandRunner;

        public PowerShellTaskFactory(IOutputService outputService, ICommandErrorReporter errorReporter, ICommandLogParser logParser)
            : this(outputService, errorReporter, logParser, new PowerShellCommandRunner())
        {
        }

        public PowerShellTaskFactory(IOutputService outputService, ICommandErrorReporter errorReporter, ICommandLogParser logParser, IShellCommandRunner commandRunner)
        {
            this.outputService = outputService;
            this.errorReporter = errorReporter;
            this.logParser = logParser;
            this.commandRunner = commandRunner;
        }

        public Task Create(ShellCommand command)
        {
            return new Task(() =>
                {
                    CommandOutput commandOutput;
                    CommandResult output = new CommandResult("", "", "");
                    try
                    {
                        output = this.commandRunner.RunCommand(command);
                        var resultLog = this.logParser.Parse(output.Result);
                        var outputLog = this.logParser.Parse(output.Output);
                        var errorLog = this.logParser.Parse(output.Error);

                        commandOutput = resultLog;
                        commandOutput.Results.AddRange(outputLog.Results);
                        commandOutput.Results.AddRange(errorLog.Results.Select(error =>
                            {
                                error.Type = CommandOutputItemType.Error;
                                return error;
                            }));

                    }
                    catch (Exception ex)
                    {
                        commandOutput = new CommandOutput("goose", "Failed to run command: " + command.Command, ex.ToString(), CommandOutputItemType.Error);
                        output = new CommandResult(commandOutput.ToString(), "", "");
                    }

                    this.errorReporter.Report(command, output);
                    this.outputService.Handle(commandOutput);    
                });
        }        
    }

}
