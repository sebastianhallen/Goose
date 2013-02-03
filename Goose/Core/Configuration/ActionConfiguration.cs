namespace Goose.Core.Configuration
{
    public class ActionConfiguration
    {

        public Trigger Trigger { get; private set; }
        public string WorkingDirectory { get; private set; }
        public string Command { get; private set; }
        public string ProjectRoot { get; private set; }

        public ActionConfiguration(string projectPath)
        {
            this.ProjectRoot = projectPath;
            this.Trigger = Trigger.Unknown;            
        }

        public ActionConfiguration(Trigger trigger, string workingDirectory, string command, string projectRoot)
        {
            this.Trigger = trigger;
            this.WorkingDirectory = workingDirectory;
            this.Command = command;
            this.ProjectRoot = projectRoot;
        }

        public bool IsValid
        {
            get
            {
                return 
                    this.Trigger == Trigger.Save 
                    && this.WorkingDirectory != null
                    && !string.IsNullOrWhiteSpace(this.Command);

            }
        }

        public Shell Shell
        {
            get { return Shell.PowerShell; }
        }

        public string Glob
        {
            get { return "*.less"; }
        }
    }
}