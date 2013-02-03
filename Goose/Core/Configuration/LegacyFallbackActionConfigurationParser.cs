namespace Goose.Core.Configuration
{
    using System;
    using System.IO;

    public class LegacyFallbackActionConfigurationParser
    {
        private readonly ActionConfigurationParser actionConfigurationParser = new ActionConfigurationParser();
        private readonly LegacyConfigurationParser legacyConfigurationParser = new LegacyConfigurationParser();

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

        public ActionConfiguration Parse(string projectRoot, string input)
        {
            var config = this.actionConfigurationParser.Parse(projectRoot, input);
            if (config.IsValid)
            {
                return config;
            }

            var legacyConfig = this.legacyConfigurationParser.Parse(projectRoot, input);
            return legacyConfig.IsValid ? legacyConfig : config;
        }
    }
}
