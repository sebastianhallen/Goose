namespace Goose.Core.Configuration
{
    using System;
    using System.IO;

    public class LegacyFallbackActionConfigurationParser
    {
        private readonly ActionConfigurationParser actionConfigurationParser = new ActionConfigurationParser();
        private readonly LegacyConfigurationParser legacyConfigurationParser = new LegacyConfigurationParser();

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

        public ActionConfiguration Parse(string input)
        {
            var config = this.actionConfigurationParser.Parse(input);
            if (config.IsValid)
            {
                return config;
            }

            var legacyConfig = this.legacyConfigurationParser.Parse(input);
            return legacyConfig.IsValid ? legacyConfig : config;
        }
    }
}
