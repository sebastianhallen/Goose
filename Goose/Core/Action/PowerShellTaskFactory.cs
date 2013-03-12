﻿namespace Goose.Core.Action
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Runspaces;
    using System.Text;
    using System.Threading.Tasks;
    using Output;
    using System.Linq;
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

        public Task Create(string command)
        {
            return new Task(() =>
            {
                var output = this.RunPowerShellCommand(command);
                this.outputService.Handle(output);
            });
        }

        private CommandOutput RunPowerShellCommand(string rawCommand)
        {
            var output = new StringBuilder();
            var errors = new List<Object>();
            try
            {
                System.Diagnostics.Debug.WriteLine(rawCommand);
                using (var runspace = RunspaceFactory.CreateRunspace())
                {
                    var command = new Command(rawCommand, isScript: true);
                    
                    runspace.Open();                    
                    var pipeline = runspace.CreatePipeline();
                    pipeline.Commands.Add(command);
                    pipeline.Commands.Add("Out-String");
                    
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
    }
}
