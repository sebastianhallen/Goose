namespace Goose.Core.Action.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using Goose.Core.Action.PowerShell.Host;

    public class PowerShellCommandRunner
        : IShellCommandRunner
    {       
        public string RunCommand(ShellCommand command)
        {
            var host = new GoosePSHost();
            var results = new List<string>();
            using (var runspace = RunspaceFactory.CreateRunspace(host))
            {
                var setWorkingDirectory = new Command("set-location");
                setWorkingDirectory.Parameters.Add("path", command.WorkingDirectory);                
                var redirectOutput = new Command("out-string");

                runspace.Open();
                var pipeline = runspace.CreatePipeline();
                pipeline.Commands.Add(setWorkingDirectory);
                pipeline.Commands.AddScript(command.Command);
                pipeline.Commands.Add(redirectOutput);

                
                foreach (var psObject in pipeline.Invoke())
                {
                    var result = FormatCommandResult(psObject);
                    results.Add(result);
                }
                runspace.Close();
            }

            return BuildOutput(results, host);
        }

        private static string BuildOutput(IEnumerable<string> results, GoosePSHost host)
        {
            var output = string.Join(Environment.NewLine, results);
            if (string.IsNullOrWhiteSpace(output))
            {
                output = string.Join(Environment.NewLine, host.Output);
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                output = string.Join(Environment.NewLine, host.Error);
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                output = string.Join(Environment.NewLine, host.Other);
            }

            return output.Trim();
        }

        private static string FormatCommandResult(PSObject psObject)
        {
            var result = string.Join(Environment.NewLine,
                                     psObject.ToString()
                                             .Split('\n')
                                             .Select(row => row.TrimEnd())

                );
            return result;
        }
    }
}