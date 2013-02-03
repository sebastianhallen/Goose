namespace Goose.Core.Configuration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class ActionConfigurationParser
    {
        public ActionConfiguration Parse(string projectRoot, Stream configStream)
        {
            try
            {
                using (var reader = new StreamReader(configStream))
                {
                    return this.Parse(projectRoot, reader.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return new ActionConfiguration(projectRoot);
            }
        }

        public virtual ActionConfiguration Parse(string projectRoot, string configContent)
        {
            try
            {
                var xml = XDocument.Parse(configContent);
                return (from action in xml.Element("goose").Elements("action")
                        let trigger = action.Attribute("on")
                        let glob = action.Attribute("glob")
                        let workingDirectory = action.Descendants("working-directory").SingleOrDefault()
                        let command = action.Descendants("command").SingleOrDefault()
                        select this.CreateCommandConfiguration(
                            projectRoot,
                            trigger == null ? null : trigger.Value,
                            glob == null ? null : glob.Value,
                            workingDirectory == null ? null : workingDirectory.Value, 
                            command == null ? null : command.Value))
                        .Single();
            }
            catch (Exception)
            {
                return new ActionConfiguration(projectRoot);
            }
        }

        protected ActionConfiguration CreateCommandConfiguration(string projectRoot, string triggerRaw, string glob, string workingDirectory, string command)
        {
            Trigger trigger;
            if (!Enum.TryParse(triggerRaw, true, out trigger))
            {
                trigger = Trigger.Unknown;
            }

            return new ActionConfiguration(trigger, glob, workingDirectory, command, projectRoot);
        }
    }
}
