namespace Goose.Core.Action
{
    using Configuration;
    using System.Collections.Generic;

    public class GooseActionFactory
        : IGooseActionFactory
    {
        private readonly IPowerShellTaskFactory powerShellTaskFactory;

        public GooseActionFactory(IPowerShellTaskFactory powerShellTaskFactory)
        {
            this.powerShellTaskFactory = powerShellTaskFactory;
        }

        public IEnumerable<IGooseAction> Create(ActionConfiguration configuration, IEnumerable<string> files)
        {
            if (configuration.IsValid)
            {
                foreach (var file in files)
                {
                    yield return new PowerShellGooseAction(
                        this.powerShellTaskFactory,
                        configuration.ProjectRoot,
                        configuration.WorkingDirectory,
                        configuration.Command);

                    if (!CommandScope.File.Equals(configuration.Scope))
                    {
                        yield break;
                    }
                }                
            }            
        }
    }
}