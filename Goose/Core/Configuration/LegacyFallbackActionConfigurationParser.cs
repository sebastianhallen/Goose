namespace Goose.Core.Configuration
{
    public class LegacyFallbackActionConfigurationParser
    {
        private readonly ActionConfigurationParser actionConfigurationParser = new ActionConfigurationParser();
        private readonly LegacyConfigurationParser legacyConfigurationParser = new LegacyConfigurationParser();

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
