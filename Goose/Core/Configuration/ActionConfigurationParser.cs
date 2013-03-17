namespace Goose.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public abstract class ActionConfigurationParser
    {
        private string projectRoot;
        protected abstract IEnumerable<ActionConfiguration> Parse(XElement gooseConfigRootNode);

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
                return new[] { new ActionConfiguration(projectRoot) };
            }
        }        

        public IEnumerable<ActionConfiguration> Parse(string projectRoot, string configContent)
        {
            this.projectRoot = projectRoot;
            try
            {
                var xml = XDocument.Parse(configContent);
                var rootNode = xml.Element("goose");
                rootNode = rootNode ?? xml.Element("compile-less");
                return this.Parse(rootNode);
            }
            catch (Exception)
            { }

            return new[] { new ActionConfiguration(projectRoot) };
        }

        protected ActionConfiguration CreateCommandConfiguration(string trigger, string glob, string workingDirectory, string command, string scope = null)
        {
            Trigger triggerValue;
            if (!Enum.TryParse(trigger, true, out triggerValue))
            {
                triggerValue = Trigger.Unknown;
            }
            CommandScope scopeValue;
            if (!Enum.TryParse(scope, true, out scopeValue))
            {
                scopeValue = CommandScope.Project;
            }

            return new ActionConfiguration(triggerValue, glob, workingDirectory, command, this.projectRoot, scopeValue);
        }
    }
}
