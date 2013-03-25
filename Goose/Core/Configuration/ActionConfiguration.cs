namespace Goose.Core.Configuration
{
    public class ActionConfiguration
    {

        public Trigger Trigger { get; private set; }
        public string RelativeWorkingDirectory { get; private set; }
        public string Command { get; private set; }
        public string ProjectRoot { get; private set; }
        public string SolutionRoot { get; private set; }
        public string Glob { get; private set; }
        public CommandScope Scope { get; private set; }

        public Shell Shell
        {
            get { return Shell.PowerShell; }
        }

        public ActionConfiguration(Trigger trigger, string glob, string workingDirectory, string command, string solutionRoot, string projectRoot, CommandScope scope)
        {
            this.Trigger = trigger;
            this.Glob = glob;
            this.RelativeWorkingDirectory = workingDirectory;
            this.Command = command;
            this.SolutionRoot = solutionRoot;
            this.ProjectRoot = projectRoot;
            this.Scope = scope;
        }

        public bool IsValid
        {
            get
            {
                return 
                    this.Trigger == Trigger.Save 
                    && !string.IsNullOrWhiteSpace(this.Glob)
                    && this.RelativeWorkingDirectory != null
                    && !string.IsNullOrWhiteSpace(this.Command);

            }
        }
    }
}