namespace Goose.Core.Configuration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class ConfigurationParser
    {
        public ActionConfiguration Parse(Stream configStream)
        {
            try
            {
                using (var reader = new StreamReader(configStream))
                {
                    return this.Parse(reader.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return new ActionConfiguration();
            }
        }

        public ActionConfiguration Parse(string configContent)
        {
            try
            {
                var xml = XDocument.Parse(configContent);
                return (from action in xml.Elements("action")
                        let trigger = action.Attribute("on")
                        let workingDirectory = action.Descendants("working-directory").SingleOrDefault()
                        let command = action.Descendants("command").SingleOrDefault()
                        select this.CreateCommandConfiguration(
                            trigger == null ? null : trigger.Value, 
                            workingDirectory == null ? null : workingDirectory.Value, 
                            command == null ? null : command.Value))
                        .SingleOrDefault();
            }
            catch (Exception)
            {
                return new ActionConfiguration();
            }
        }

        private ActionConfiguration CreateCommandConfiguration(string triggerRaw, string workingDirectory, string command)
        {
            Trigger trigger;
            if (!Enum.TryParse(triggerRaw, true, out trigger))
            {
                trigger = Trigger.Unknown;
            }

            return new ActionConfiguration(trigger, workingDirectory, command);
        }
    }
}
