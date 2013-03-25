namespace Goose.Core.Configuration
{
    public class ActionConfigurationBuilder
    {
        private Trigger trigger;
        private string workingDirectory;
        private string command;
        private string projectRoot;
        private string glob;
        private CommandScope scope;

        public ActionConfigurationBuilder On(Trigger trigger)
        {
            this.trigger = trigger;
            return this;
        }

        public ActionConfigurationBuilder FilesMatching(string glob)
        {
            this.glob = glob;
            return this;
        }

        public ActionConfigurationBuilder Run(string command)
        {
            this.command = command;
            return this;
        }

        public ActionConfigurationBuilder In(string workingDirectory)
        {
            this.workingDirectory = workingDirectory;
            return this;
        }

        public ActionConfigurationBuilder WithScope(CommandScope scope)
        {
            this.scope = scope;
            return this;
        }

        public ActionConfigurationBuilder ForProjectIn(string projectRoot)
        {
            this.projectRoot = projectRoot;
            return this;
        }

        public ActionConfiguration Build()
        {
            return new ActionConfiguration(
                this.trigger,
                this.glob,
                this.workingDirectory,
                this.command,
                this.projectRoot,
                this.scope);
        }
    }
}