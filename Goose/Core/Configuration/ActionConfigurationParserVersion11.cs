namespace Goose.Core.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class ActionConfigurationParserVersion11
        : ActionConfigurationParser
    {
        protected override IEnumerable<ActionConfiguration> Parse(XElement gooseConfigRootNode)
        {
            return (from action in gooseConfigRootNode.Elements("action")
                    let trigger = action.Attribute("on")
                    let glob = action.Attribute("glob")
                    let workingDirectory = action.Descendants("working-directory").SingleOrDefault()
                    let command = action.Descendants("command").SingleOrDefault()
                    let scope = action.Descendants("scope").SingleOrDefault()
                    select this.CreateCommandConfiguration(
                        trigger: trigger == null ? null : trigger.Value,
                        glob: glob == null ? null : glob.Value,
                        workingDirectory: workingDirectory == null ? null : workingDirectory.Value,
                        command: command == null ? null : command.Value,
                        scope: scope == null ? "project" : scope.Value));
        }
    }
}
