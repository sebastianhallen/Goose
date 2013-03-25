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

        public IEnumerable<ActionConfiguration> Parse(string solutionRoot, string projectRoot, Stream configStream)
        {
            try
            {
                using (var reader = new StreamReader(configStream))
                {
                    return this.Parse(solutionRoot, projectRoot, reader.ReadToEnd());
                }
            }
            catch (Exception)
            {
            }


            return Enumerable.Empty<ActionConfiguration>();
        }

        public IEnumerable<ActionConfiguration> Parse(string solutionRoot, string projectRoot, string input)
        {
            var actions = this.actionConfigurationParser10.Parse(solutionRoot, projectRoot, input);
            if (actions.Any(action => action.IsValid))
            {
                return actions.Where(action => action.IsValid);
            }

            var legacyConfig = this.legacyConfigurationParser.Parse(solutionRoot, projectRoot, input);
            return legacyConfig.Any(config => config.IsValid) ? legacyConfig.Where(config => config.IsValid) : actions;
        }
    }
}
