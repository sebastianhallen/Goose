namespace Goose.Core.Action
{
    using System;
    using System.Collections.Generic;
    using Configuration;

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
                return new [] {
                    new PowerShellGooseAction(
                        this.powerShellTaskFactory,
                        configuration.ProjectRoot,
                        configuration.WorkingDirectory,
                        configuration.Command)
                };
            }

            return new[] {new VoidGooseAction()};
        }
    }
}