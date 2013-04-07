namespace Goose.Core.Action.PowerShell
{
    using System;
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
                    CommandOutput output;
                    try
                    {
                        var rawOutput = this.commandRunner.RunCommand(command);
                        output = this.logParser.Parse(rawOutput);
                    }
                    catch (Exception ex)
                    {
                        output = new CommandOutput("goose", "Failed to run command: " + command.Command, ex.ToString(), CommandOutputItemType.Error, ex);
                    }
                                        
                    this.outputService.Handle(output);    
                });
        }        
    }
}
