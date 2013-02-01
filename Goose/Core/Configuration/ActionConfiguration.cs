namespace Goose.Core.Configuration
{
    public class ActionConfiguration
    {

        public Trigger Trigger { get; private set; }
        public string WorkingDirectory { get; private set; }
        public string Command { get; private set; }

        public ActionConfiguration()
        {
            this.Trigger = Trigger.Unknown;            
        }

        public ActionConfiguration(Trigger trigger, string workingDirectory, string command)
        {
            this.Trigger = trigger;
            this.WorkingDirectory = workingDirectory;
            this.Command = command;
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

    }
}