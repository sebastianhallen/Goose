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
        private ActionConfigurationBuilder configurationBuilder;

        [SetUp]
        public void Before()
        {
            this.commandBuilder = new PowerShellCommandBuilder();
            this.configurationBuilder = new ActionConfigurationBuilder();            
        }

        [Test]
        public void Should_set_working_directory()
        {
            var configuration = this.configurationBuilder
                             .On(Trigger.Save).FilesMatching("glob")
                             .Run("command").In("working-directory").ForProjectIn("root-directory")
                             .WithScope(CommandScope.Project)
                             .Build();
            
            var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.WorkingDirectory, Is.EqualTo(@"root-directory\working-directory"));
        }

        [Test]
        public void Should_set_command()
        {
            var configuration = this.configurationBuilder
                             .On(Trigger.Save).FilesMatching("glob")
                             .Run("command").In("working-directory").ForProjectIn("root-directory")
                             .WithScope(CommandScope.Project)
                             .Build();
            
            var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.Command, Is.EqualTo("command"));
        }

        [Test]
        public void Should_be_able_to_parameterize_absolute_file_path()
        {
            var configuration = this.configurationBuilder
                             .On(Trigger.Save).FilesMatching("glob")
                             .Run("command with {absolute-file-path} substitution").In("working-directory").ForProjectIn("root-directory")
                             .WithScope(CommandScope.Project)
                             .Build();      
            var environmentVariables = new CommandEvironmentVariables(@"absolute\file-path");

            var command = this.commandBuilder.Build(configuration, environmentVariables);

            Assert.That(command.Command, Is.EqualTo(@"command with absolute\file-path substitution"));           
        }

        [Test]
        public void Should_be_able_to_parameterize_relative_file_path()
        {
            var environmentVariables = new CommandEvironmentVariables(@"project-root\file-path");
            var configuration = this.configurationBuilder
                       .On(Trigger.Save).FilesMatching("glob")
                       .Run("command with {relative-file-path} substitution").In("working-directory").ForProjectIn("project-root")
                       .WithScope(CommandScope.Project)
                       .Build();
      

            var command = this.commandBuilder.Build(configuration, environmentVariables);

            Assert.That(command.Command, Is.EqualTo(@"command with file-path substitution"));
        }

        [Test]
        public void Should_be_able_to_parameterize_project_root_path()
        {
            var configuration = this.configurationBuilder
                   .On(Trigger.Save).FilesMatching("glob")
                   .Run("command with {project-root} substitution").In("working-directory").ForProjectIn(@"project-root\path")
                   .WithScope(CommandScope.Project)
                   .Build();

            var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.Command, Is.EqualTo(@"command with project-root\path substitution")); 
        }

        [Test]
        public void Should_be_able_to_parameterize_solution_root_path()
        {
            var configuration = this.configurationBuilder
                   .ForProjectIn(@"project-root\path").ProjectInSolution(@"solution-root\path").On(Trigger.Save).FilesMatching("glob")
                   .Run("command with {solution-root} substitution").In("working-directory")
                   .WithScope(CommandScope.Project)
                   .Build();

            var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.Command, Is.EqualTo(@"command with solution-root\path substitution"));
        }

        [Test]
        public void Should_be_able_to_parameterize_absolute_working_directory_path()
        {
            var configuration = this.configurationBuilder
               .On(Trigger.Save).FilesMatching("glob")
               .Run("command with {working-directory} substitution").In("working-directory").ForProjectIn(@"project-root\path")
               .WithScope(CommandScope.Project)
               .Build();

            var command = this.commandBuilder.Build(configuration, new CommandEvironmentVariables(""));

            Assert.That(command.Command, Is.EqualTo(@"command with project-root\path\working-directory substitution"));
        }

    }
}
