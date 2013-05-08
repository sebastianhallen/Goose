namespace Goose.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class LegacyConfigurationParser
        : ActionConfigurationParser
    {
        protected override IEnumerable<ActionConfiguration> Parse(XElement gooseConfigRootNode)
        {
            var workingDirectory = gooseConfigRootNode.Descendants("build-directory").SingleOrDefault();
            var command = gooseConfigRootNode.Descendants("compile-command").SingleOrDefault();
            return new[] { this.CreateCommandConfiguration(                        
                        trigger: "save",
                        glob: "*.less",
                        workingDirectory : workingDirectory == null ? null : workingDirectory.Value,
                        command: command == null ? null : command.Value)}; 
        }       
    }
}
