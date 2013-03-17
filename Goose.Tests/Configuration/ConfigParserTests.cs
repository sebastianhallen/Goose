namespace Goose.Tests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    public abstract class ConfigParserTests
    {
        protected abstract string Version { get; }
        
        protected string CreateConfig(Action<GooseConfigRootBuilder> configure)
        {
            var configBuilder = new GooseTestConfigBuilder();
            var configurator = configBuilder.Version(this.Version);

            configure(configurator);

            return configBuilder.Build();
        }

        protected class GooseTestConfigBuilder
        {
            private GooseConfigRootBuilder root;

            public string Build()
            {
                if (this.root == null)
                {
                    throw new InvalidOperationException("You need to configure the config first.");
                }

                return (this.root as IGooseConfigRootBuilderInternals).CreateSnapshot().ToString();
            }

            public GooseConfigRootBuilder Version(string version) 
            {
                this.root = new GooseConfigRootBuilder(version);
                return this.root;
            }   
        }

        internal interface IGooseConfigActionBuilderInternals
        {
            XElement Build();
        }

        public class GooseConfigActionBuilder
            : IGooseConfigActionBuilderInternals
        {
            private string trigger;
            private string glob;
            private string workingDirectory;
            private string command;

            public GooseConfigActionBuilder TriggersOn(string trigger)
            {
                this.trigger = trigger;
                return this;
            }

            public GooseConfigActionBuilder ForFilesMatching(string glob)
            {
                this.glob = glob;
                return this;
            }

            public GooseConfigActionBuilder WithWorkingDirectory(string workingDirectory)
            {
                this.workingDirectory = workingDirectory;
                return this;
            }

            public GooseConfigActionBuilder WithCommand(string command)
            {
                this.command = command;
                return this;
            }

            XElement IGooseConfigActionBuilderInternals.Build()
            {
                var actionXml = new XElement("action");
                if (this.trigger != null) actionXml.SetAttributeValue("on", this.trigger);
                if (this.glob != null) actionXml.SetAttributeValue("glob", this.glob);
                if (this.workingDirectory != null) actionXml.Add(new XElement("working-directory", this.workingDirectory));
                if (this.command != null) actionXml.Add(new XElement("command", this.command));

                return actionXml;
            }
        }

        internal interface IGooseConfigRootBuilderInternals
        {
            XElement CreateSnapshot();
        }

        public class GooseConfigRootBuilder
            : IGooseConfigRootBuilderInternals
        {
            private readonly string version;
            private readonly IList<XElement> actions;
            public GooseConfigRootBuilder(string version)
            {
                this.version = version;
                this.actions = new List<XElement>();
            }

            public GooseConfigRootBuilder WithAction(Action<GooseConfigActionBuilder> configuration)
            {
                var action = new GooseConfigActionBuilder();
                configuration(action);
                this.actions.Add((action as IGooseConfigActionBuilderInternals).Build());

                return this;
            }

            XElement IGooseConfigRootBuilderInternals.CreateSnapshot()
            {
                var root = new XElement("goose", new XAttribute("version", this.version));
                foreach (var action in this.actions)
                {
                    root.Add(action);
                }

                return root;
            }
        }
    }
}
