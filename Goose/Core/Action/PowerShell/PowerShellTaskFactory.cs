namespace Goose.Core.Action.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Runspaces;
    using System.Text;
    using System.Threading.Tasks;
    using Goose.Core.Output;

    public class PowerShellTaskFactory
        : IPowerShellTaskFactory
    {
        private readonly IOutputService outputService;
        private readonly ICommandLogParser logParser;

        public PowerShellTaskFactory(IOutputService outputService, ICommandLogParser logParser)
        {
            this.outputService = outputService;
            this.logParser = logParser;
        }

        public Task Create(ShellCommand command)
        {
            return new Task(() =>
                {
                    var output = this.RunPowerShellCommand(command);
                    this.outputService.Handle(output);    
                });
        }

        private CommandOutput RunPowerShellCommand(ShellCommand command)
        {
            var output = new StringBuilder();
            var errors = new List<Object>();
            try
            {                
                using (var runspace = RunspaceFactory.CreateRunspace())
                {
                    var setWorkingDirectory = new Command("set-location");
                    setWorkingDirectory.Parameters.Add("path", command.WorkingDirectory);

                    var payloadCommand = new Command(command.Command, isScript: true);
                    var redirectOutput = new Command("out-string");

                    runspace.Open();
                    var pipeline = runspace.CreatePipeline();
                    pipeline.Commands.Add(setWorkingDirectory);
                    pipeline.Commands.Add(payloadCommand);
                    pipeline.Commands.Add(redirectOutput);

                    foreach (var result in pipeline.Invoke())
                    {
                        errors.AddRange(pipeline.Error.ReadToEnd());
                        output.AppendFormat("{0}", result);
                        System.Diagnostics.Debug.WriteLine(result);
                    }

                    runspace.Close();
                }
            }
            catch (Exception ex)
            {
                return new CommandOutput("goose", "Failed to run compile command", ex.ToString(), CommandOutputItemType.Error);
            }

            return this.logParser.Parse(output.ToString());
        }

        //private CommandOutput RunPowerShellCommand(string rawCommand)
        //{
        //    var output = new StringBuilder();
        //    var errors = new List<Object>();
        //    try
        //    {
        //        System.Diagnostics.Debug.WriteLine(rawCommand);
        //        using (var runspace = RunspaceFactory.CreateRunspace())
        //        {
        //            var command = new Command(rawCommand, isScript: true);
                    
        //            runspace.Open();                    
        //            var pipeline = runspace.CreatePipeline();
                    
        //            pipeline.Commands.Add(command);
        //            pipeline.Commands.Add("Out-String");
                    
        //            foreach (var result in pipeline.Invoke())
        //            {                        
        //                errors.AddRange(pipeline.Error.ReadToEnd());
        //                output.AppendFormat("{0}", result);
        //                System.Diagnostics.Debug.WriteLine(result);
        //            }

        //            runspace.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new CommandOutput("goose", "Failed to run compile command", ex.ToString(), CommandOutputItemType.Error);
        //    }

        //    return this.logParser.Parse(output.ToString());
        //}
    }
}
