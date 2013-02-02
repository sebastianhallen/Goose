namespace Goose.Core.Solution.EventHandling
{
    public interface IGlobMatcher
    {
        bool Matches(string fileName, string glob);
    }
}