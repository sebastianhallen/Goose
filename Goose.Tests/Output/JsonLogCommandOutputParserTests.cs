﻿using System;

namespace Goose.Tests.Output
{
    using System.Linq;
    using Goose.Core.Output;
    using NUnit.Framework;

    [TestFixture]
    public class JsonLogCommandOutputParserTests
    {
        private ICommandLogParser logParser;

        [SetUp]
        public void Before()
        {
            this.logParser = new JsonCommandLogParser();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public void Should_create_empty_command_output_with_info_about_completion_with_no_about(string input)
        {
            var result = this.logParser.Parse(input);

            Assert.That(result.Results.Any(), Is.False);
        }

        [Test]
        public void Should_be_able_to_handle_a_json_serialized_log()
        {
            var log = @"{
		   ""version"": 1,
		   ""name"": ""lessify"",
		   ""time"": ""2013-01-14T09:19:40.786Z"",
		   ""results"": [
		     {
		       ""type"": ""error"",
		       ""message"": ""Syntax Error on line 6"",
		       ""excerpt"": "" 5:     position: absolute;\n 6:     t op: 70px;\n 7:     bottom: 0;"",
		       ""line"": 6,
		       ""filename"": ""assets/datamodelviewer/datamodelviewer.less""
		     },
		     {
		       ""type"": ""message"",
		       ""message"": ""This is now done.""
		     }
		   ]
		 }";

            var result = this.logParser.Parse(log);

            var message = result.Results.Last().Message;
            Assert.That(message, Is.EqualTo("This is now done."));
        }

        [Test]
        public void Should_log_to_message_log_when_unable_to_parse_log_input()
        {
            var log = "JOIADFNJOisfogäpjädfghoöpsdifågj'åkdfåp¨)R#Y)€(#";

            var result = this.logParser.Parse(log);

            Assert.That(result.Results.Last().Message, Is.EqualTo(log));
        }     
    }
}
