namespace Goose.Tests.Configuration
{
    using Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ActionConfigurationTests
    {
        private ActionConfigurationBuilder configBuilder;

        [SetUp]
        public void Before()
        {
            this.configBuilder = new ActionConfigurationBuilder();
        }

        [Test]
        public void Should_set_project_path()
        {
            var config = this.configBuilder
               .On(Trigger.Save).FilesMatching("glob")
               .Run("some command").In("working-directory").ForProjectIn(@"project-root\path")
               .WithScope(CommandScope.Project)
               .Build();
        
            Assert.That(config.ProjectRoot, Is.EqualTo(@"project-root\path"));
        }

        [Test]
        public void Save_configuration_with_non_empty_command_should_be_valid()
        {
            var config = this.configBuilder
               .On(Trigger.Save).FilesMatching("glob")
               .Run("some-command").In("working-directory").ForProjectIn(@"project-root\path")
               .WithScope(CommandScope.Project)
               .Build();
            
            Assert.That(config.IsValid);
        }

        [Test]
        public void Save_configuration_with_empty_command_should_not_be_valid()
        {
            var config = this.configBuilder
               .On(Trigger.Save).FilesMatching("glob")
               .Run("").In("working-directory").ForProjectIn(@"project-root\path")
               .WithScope(CommandScope.Project)
               .Build();
        
            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Unknown_configuration_should_not_be_valid()
        {
            var config = this.configBuilder
               .On(Trigger.Unknown).FilesMatching("glob")
               .Run("some command").In("working-directory").ForProjectIn(@"project-root\path")
               .WithScope(CommandScope.Project)
               .Build();
          
            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Configuration_should_not_be_valid_when_working_directory_is_not_set()
        {
            var config = this.configBuilder
               .On(Trigger.Save).FilesMatching("glob")
               .Run("some command").In(null).ForProjectIn(@"project-root\path")
               .WithScope(CommandScope.Project)
               .Build();
        
            Assert.That(config.IsValid, Is.False);
        }

        [Test]
        public void Configuration_should_not_be_valid_when_glob_is_empty()
        {
            var config = this.configBuilder
               .On(Trigger.Save).FilesMatching("")
               .Run("some command").In("working-directory").ForProjectIn(@"project-root\path")
               .WithScope(CommandScope.Project)
               .Build();
        
            Assert.That(config.IsValid, Is.False);
        }
    }
}