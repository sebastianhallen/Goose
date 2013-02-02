namespace Goose.Core.Solution
{
    public interface IGlobMatcher
    {
        bool Matches(string fileName, string glob);
    }
}