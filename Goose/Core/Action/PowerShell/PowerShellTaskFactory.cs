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
        private readonly ICommandLogParser logParser;
        private readonly IShellCommandRunner commandRunner;

        public PowerShellTaskFactory(IOutputService outputService, ICommandLogParser logParser)
            : this(outputService, logParser, new PowerShellCommandRunner())
        {
        }

        public PowerShellTaskFactory(IOutputService outputService, ICommandLogParser logParser, IShellCommandRunner commandRunner)
        {
            this.outputService = outputService;
            this.logParser = logParser;
            this.commandRunner = commandRunner;
        }

        public Task Create(ShellCommand command)
        {
            return new Task(() =>
                {
                    CommandOutput commandOutput;
                    try
                    {
                        var rawOutput = this.commandRunner.RunCommand(command);
                        var result = this.logParser.Parse(rawOutput.Result);
                        var output = this.logParser.Parse(rawOutput.Output);
                        var errors = this.logParser.Parse(rawOutput.Error);

                        commandOutput = result;
                        commandOutput.Results.AddRange(output.Results);
                        commandOutput.Results.AddRange(errors.Results.Select(error =>
                            {
                                error.Type = CommandOutputItemType.Error;
                                return error;
                            }));

                    }
                    catch (Exception ex)
                    {
                        commandOutput = new CommandOutput("goose", "Failed to run command: " + command.Command, ex.ToString(), CommandOutputItemType.Error);
                    }

                    this.outputService.Handle(commandOutput);    
                });
        }        
    }

}
