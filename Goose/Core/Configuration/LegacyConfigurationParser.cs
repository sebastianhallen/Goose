﻿namespace Goose.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class LegacyConfigurationParser
        : ActionConfigurationParser
    {
        public override IEnumerable<ActionConfiguration> Parse(string projectRoot, string configContent)
        {
            try
            {
                var xml = XDocument.Parse(configContent);
                return (from action in xml.Elements("compile-less")
                        let workingDirectory = action.Descendants("build-directory").SingleOrDefault()
                        let command = action.Descendants("compile-command").SingleOrDefault()
                        select this.CreateCommandConfiguration(
                            projectRoot,
                            "save",
                            "*.less",
                            workingDirectory == null ? null : workingDirectory.Value,
                            command == null ? null : command.Value));
            }
            catch (Exception)
            {
                return new [] { new ActionConfiguration(projectRoot) };
            }
        }
    }
}