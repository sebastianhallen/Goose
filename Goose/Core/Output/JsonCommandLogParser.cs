﻿namespace Goose.Core.Output
{
    using System;
    using System.Web.Script.Serialization;

    public class JsonCommandLogParser
        : ICommandLogParser
    {
        private readonly JavaScriptSerializer serializer;

        public JsonCommandLogParser()
        {
            this.serializer = new JavaScriptSerializer();
        }

        public CommandOutput Parse(string buildLog)
        {
            if (string.IsNullOrWhiteSpace(buildLog))
            {
                return new CommandOutput();
            }

            CommandOutput result;
            try
            {
                result = this.serializer.Deserialize<CommandOutput>(buildLog);
            }
            catch (Exception ex)
            {
                result = new CommandOutput("goose", buildLog, "", CommandOutputItemType.Message);
            }

            return result;
        }
    }
}