namespace Goose.Tests.Action
{
    using Goose.Core.Action;
    using Goose.Core.Action.PowerShell;
    using Goose.Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class PowerShellCommandBuilderTests
    {
        private PowerShellCommandBuilder commandBuilder;

        [SetUp]
        public void Before()
        {
            this.commandBuilder = new PowerShellCommandBuilder();
        }

        [Test]
        public void Should_set_working_directory()
        {
           var configuration = new ActionConfiguration(Trigger.Save, "", "working-directory", "", "", CommandScope.Project);

           var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.WorkingDirectory, Is.EqualTo("working-directory"));
        }

        [Test]
        public void Should_set_command()
        {
            var configuration = new ActionConfiguration(Trigger.Save, "", "", "some command", "", CommandScope.Project);

            var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.Command, Is.EqualTo("some command"));
        }

        [Test]
        public void Should_be_able_to_parameterize_absolute_file_path()
        {
            var configuration = new ActionConfiguration(Trigger.Save, "", "", "command with {absolute-file-path} substitution", "", CommandScope.Project);
            var environmentVariables = new CommandEvironmentVariables(@"absolute\file-path");

            var command = this.commandBuilder.Build(configuration, environmentVariables);

            Assert.That(command.Command, Is.EqualTo(@"command with absolute\file-path substitution"));           
        }

        [Test]
        public void Should_be_able_to_parameterize_relative_file_path()
        {
            var configuration = new ActionConfiguration(Trigger.Save, "", "working-directory", "command with {relative-file-path} substitution", "project-root", CommandScope.Project);
            var environmentVariables = new CommandEvironmentVariables(@"project-root\file-path");

            var command = this.commandBuilder.Build(configuration, environmentVariables);

            Assert.That(command.Command, Is.EqualTo(@"command with file-path substitution"));
        }

        [Test]
        public void Should_be_able_to_parameterize_project_root_path()
        {
            var configuration = new ActionConfiguration(Trigger.Save, "", "", "command with {project-root} substitution", @"project-root\path", CommandScope.Project);
            
            var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.Command, Is.EqualTo(@"command with project-root\path substitution")); 
        }

        [Test]
        public void Should_be_able_to_parameterize_absolute_working_directory_path()
        {
            var configuration = new ActionConfiguration(Trigger.Save, "", "working-directory", "command with {working-directory} substitution", "project-root", CommandScope.Project);

            var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.Command, Is.EqualTo(@"command with project-root\working-directory substitution"));
        }
    }
}
