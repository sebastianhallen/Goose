namespace Goose.Core.Action.PowerShell
{
    using System.IO;
    using Goose.Core.Configuration;
    using System.Collections.Generic;

    public class PowerShellGooseActionFactory
        : IGooseActionFactory
    {
        private readonly IPowerShellTaskFactory powerShellTaskFactory;
        private readonly IShellCommandBuilder commandBuilder;

        public PowerShellGooseActionFactory(IPowerShellTaskFactory powerShellTaskFactory, IShellCommandBuilder commandBuilder)
        {
            this.powerShellTaskFactory = powerShellTaskFactory;
            this.commandBuilder = commandBuilder;
        }

        public IEnumerable<IGooseAction> Create(ActionConfiguration configuration, IEnumerable<string> files)
        {
            if (configuration.IsValid)
            {
                foreach (var file in files)
                {
                    var workingDirectory = Path.Combine(configuration.ProjectRoot, configuration.RelativeWorkingDirectory);
                    var environmentVariables = new CommandEvironmentVariables(file);
                    var command = this.commandBuilder.Build(configuration, environmentVariables);

                    yield return new PowerShellGooseAction(this.powerShellTaskFactory, command);
                    
                    if (!CommandScope.File.Equals(configuration.Scope))
                    {
                        yield break;
                    }
                }                
            }            
        }
    }
}