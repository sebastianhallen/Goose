namespace Goose.Core.Output
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
                return new CommandOutput("goose", "on save command completed without any output", "", CommandOutputItemType.Message);
            }

            CommandOutput result;
            try
            {
                result = this.serializer.Deserialize<CommandOutput>(buildLog);
            }
            catch (Exception ex)
            {
                result = new CommandOutput("goose", string.Format("unable to make sense of build log: {0}{1}", Environment.NewLine, buildLog), ex.ToString(), CommandOutputItemType.Message);
            }

            return result;
        }
    }
}