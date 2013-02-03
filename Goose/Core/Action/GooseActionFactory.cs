namespace Goose.Core.Action
{
    using Configuration;

    public class GooseActionFactory
        : IGooseActionFactory
    {
        private readonly IPowerShellTaskFactory powerShellTaskFactory;

        public GooseActionFactory(IPowerShellTaskFactory powerShellTaskFactory)
        {
            this.powerShellTaskFactory = powerShellTaskFactory;
        }

        public IGooseAction Create(ActionConfiguration configuration)
        {
            if (configuration.IsValid)
            {
                return new PowerShellGooseAction(
                    this.powerShellTaskFactory,
                    configuration.ProjectRoot,
                    configuration.WorkingDirectory,
                    configuration.Command);
            }

            return new VoidGooseAction();
        }
    }
}