namespace Goose.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class ConfigurationParser
    {
        private readonly ActionConfigurationParser actionConfigurationParser10 = new MultipleActionsConfigurationParser();
        private readonly LegacyConfigurationParser legacyConfigurationParser = new LegacyConfigurationParser();

        public IEnumerable<ActionConfiguration> Parse(string projectRoot, Stream configStream)
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
                return new [] { new ActionConfiguration(projectRoot) };
            }
        }

        public IEnumerable<ActionConfiguration> Parse(string projectRoot, string input)
        {
            var actions = this.actionConfigurationParser10.Parse(projectRoot, input);
            if (actions.Any(action => action.IsValid))
            {
                return actions.Where(action => action.IsValid);
            }

            var legacyConfig = this.legacyConfigurationParser.Parse(projectRoot, input);
            return legacyConfig.Any(config => config.IsValid) ? legacyConfig.Where(config => config.IsValid) : actions;
        }
    }
}
