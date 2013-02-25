namespace Goose.Core.Solution
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Configuration;

    public interface IConfigReader
    {
        IEnumerable<ActionConfiguration> GetActionConfigurations(ISolutionProject project);
    }

    public class DefaultConfigReader
        : IConfigReader
    {
        private LegacyFallbackActionConfigurationParser configParser;

        public DefaultConfigReader()
        {
            this.configParser = new LegacyFallbackActionConfigurationParser();
        }

        public IEnumerable<ActionConfiguration> GetActionConfigurations(ISolutionProject project)
        {
            string configPath;
            if (this.TryGetConfigPath(project, out configPath))
            {
                var projectRoot = Path.GetDirectoryName(project.ProjectFilePath);

                using (var fileStream = File.OpenRead(configPath))
                {
                    return this.configParser.Parse(projectRoot, fileStream);
                }
            }

            return Enumerable.Empty<ActionConfiguration>();
        }

        private bool TryGetConfigPath(ISolutionProject project, out string configPath)
        {
            configPath = null;
            var projectRoot = Path.GetDirectoryName(project.ProjectFilePath);
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                return false;
            }

            configPath = Path.Combine(projectRoot, "goose.config");
            return File.Exists(configPath);
        }        
    }
}
