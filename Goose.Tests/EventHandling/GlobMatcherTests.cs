namespace Goose.Tests.EventHandling
{
    using Core.Solution.EventHandling;
    using NUnit.Framework;

    [TestFixture]
    public class GlobMatcherTests
    {
        private RegexGlobMatcher regexGlobMatcher;

        [SetUp]
        public void Before()
        {
            this.regexGlobMatcher = new RegexGlobMatcher();
        }

        [TestCase("*.less", "file.less", true)]
        [TestCase("file.less", "file.less", true)]
        [TestCase("exact.less", "file.less", false)]
        [TestCase(".less", "file.less", false)]
        [TestCase(null, "file.less", false)]
        [TestCase("     ", "file.less", false)]
        [TestCase(".less", "", false)]
        [TestCase(".less", null, false)]
        public void Should_match_single_globs(string glob, string filename, bool result)
        {
            Assert.That(this.regexGlobMatcher.Matches(filename, glob), Is.EqualTo(result));
        }
    }
}
