namespace Goose.Core.Action.PowerShell
{
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
                    yield return new PowerShellGooseAction(
                        this.powerShellTaskFactory,
                        this.commandBuilder,
                        configuration.ProjectRoot,
                        file,
                        configuration.WorkingDirectory,
                        configuration.Command,
                        configuration.Scope);

                    if (!CommandScope.File.Equals(configuration.Scope))
                    {
                        yield break;
                    }
                }                
            }            
        }
    }
}