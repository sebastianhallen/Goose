namespace Goose.Tests.EventHandling
{
    using Core.Solution.EventHandling;
    using FakeItEasy;
    using Microsoft.VisualStudio.Shell.Interop;
    using NUnit.Framework;

    [TestFixture]
    public class FileChangeConsumingTests
    {
        [UnderTest] private FileMonitor fileChangeConsumer;

        [SetUp]        
        public void Before()
        {
            Fake.InitializeFixture(this);
        }

        [Test]
        public void Should_trigger_deletion_action_when_a_watched_file_is_deleted()
        {
            this.fileChangeConsumer.FilesChanged(1, new[] {"file"}, new uint[] {(uint) _VSFILECHANGEFLAGS.VSFILECHG_Del});
        }

        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Add)]
        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Size)]
        [TestCase(_VSFILECHANGEFLAGS.VSFILECHG_Time)]        
        public void Should_trigger_save_action_when_a_watched_file_is_saved(_VSFILECHANGEFLAGS changeFlag)
        {
            this.fileChangeConsumer.FilesChanged(1, new[] { "file" }, new uint[] { (uint)changeFlag});

        }
    }
}
