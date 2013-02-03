﻿namespace Goose.Core.Solution.EventHandling
{
    using System.Text.RegularExpressions;

    public class RegexGlobMatcher
        : IGlobMatcher
    {
        public bool Matches(string fileName, string glob)
        {
            var regexGlob = string.Format("^{0}$", glob
                                                       .Replace("*", ".*")
                                                       .Replace("?", "."));

            return Regex.IsMatch(fileName, regexGlob);
        }
    }
}