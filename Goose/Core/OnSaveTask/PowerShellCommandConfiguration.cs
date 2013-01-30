namespace Goose.Core.OnSaveTask
{
    using System;
    using System.IO;
    using System.Xml;

    public class PowerShellCommandConfiguration
    {
        private readonly string projectDirectory;
        private string compileCommandField;
        private string ConfigPath
        {
            get { return Path.Combine(projectDirectory, Constants.ConfigFileName); }
        }

        public string ConfigurationFailedReason { get; private set; }
        public string BuildDirectory { get; private set; }
        public string CompileCommand
        {
            get
            {
                return string.Format(@"cd ""{0}"" ; {1}", this.BuildDirectory, this.compileCommandField);
            }
            private set { this.compileCommandField = value; }
        }

        public PowerShellCommandConfiguration(string projectDirectory)
        {
            this.projectDirectory = projectDirectory;
        }

        public bool Configure()
        {
            if (!File.Exists(this.ConfigPath))
            {
                this.ConfigurationFailedReason = string.Format("Unable to find config file @ {0}", this.ConfigPath);
                return false;
            }

            try
            {
                var xml = new XmlDocument();
                xml.Load(this.ConfigPath);
                
                var buildDirectory = xml.SelectSingleNode("on-save-action/working-directory").InnerText;
                var compileLessCommand = xml.SelectSingleNode("on-save-action/powershell-command").InnerText;

                this.BuildDirectory = Path.Combine(projectDirectory, buildDirectory);
                this.CompileCommand = compileLessCommand;

            }
            catch (Exception ex)
            {
                this.ConfigurationFailedReason = string.Format("could not determine compile command from config: {0}", ex);
                return false;
            }

            return true;
        }
    }
}