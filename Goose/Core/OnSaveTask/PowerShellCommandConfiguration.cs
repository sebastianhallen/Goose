namespace Goose.Core.OnSaveTask
{
    using System;
    using System.IO;
    using Configuration;

    public class PowerShellCommandConfiguration
    {
        private readonly string projectDirectory;
        private readonly LegacyFallbackActionConfigurationParser configParser;
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
            this.configParser = new LegacyFallbackActionConfigurationParser();
        }

        public bool Configure()
        {
            var validConfiguration = false;
            try
            {

                using (var fileStream = File.Open(this.ConfigPath, FileMode.Open))
                {
                    var config = this.configParser.Parse(projectDirectory, fileStream);

                    this.BuildDirectory = Path.Combine(projectDirectory, config.WorkingDirectory);
                    this.CompileCommand = config.Command;

                    validConfiguration = config.IsValid;
                }
            }
            catch (FileNotFoundException)
            {
                this.ConfigurationFailedReason = string.Format("Unable to find config file @ {0}", this.ConfigPath);
            }
            catch (Exception ex)
            {
                this.ConfigurationFailedReason = string.Format("could not determine compile command from config: {0}", ex);                
            }

            return validConfiguration;
        }
    }
}