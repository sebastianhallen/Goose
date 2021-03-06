﻿namespace Goose.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public abstract class ActionConfigurationParser
    {
        private string projectRoot;
        private string solutionRoot;
        protected abstract IEnumerable<ActionConfiguration> Parse(XElement gooseConfigRootNode);

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
                return Enumerable.Empty<ActionConfiguration>();
            }
        }        

        public IEnumerable<ActionConfiguration> Parse(string solutionRoot, string projectRoot, string configContent)
        {
            this.projectRoot = projectRoot;
            this.solutionRoot = solutionRoot;
            try
            {
                var xml = XDocument.Parse(configContent);
                var rootNode = xml.Element("goose");
                rootNode = rootNode ?? xml.Element("compile-less");
                return this.Parse(rootNode);
            }
            catch (Exception)
            { }


            return Enumerable.Empty<ActionConfiguration>();
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

            return new ActionConfigurationBuilder()
                .ForProjectIn(this.projectRoot).ProjectInSolution(this.solutionRoot)
                .On(triggerValue).FilesMatching(glob).Run(command).WithScope(scopeValue)
                .In(workingDirectory)
            .Build();           
        }
    }
}
