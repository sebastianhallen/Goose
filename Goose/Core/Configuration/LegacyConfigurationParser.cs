namespace Goose.Core.Configuration
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    public class LegacyConfigurationParser
        : ActionConfigurationParser
    {
        public override ActionConfiguration Parse(string configContent)
        {
            try
            {
                var xml = XDocument.Parse(configContent);
                return (from action in xml.Elements("compile-less")                        
                        let workingDirectory = action.Descendants("build-directory").SingleOrDefault()
                        let command = action.Descendants("compile-command").SingleOrDefault()
                        select this.CreateCommandConfiguration(
                            "save",
                            workingDirectory == null ? null : workingDirectory.Value,
                            command == null ? null : command.Value))
                        .Single();
            }
            catch (Exception)
            {
                return new ActionConfiguration();
            }
        }
    }
}
