namespace Goose.Core.Solution
{
    using System.Collections.Generic;

    public interface ISolutionProject
    {
        string SolutionFilePath { get; }
        string ProjectFilePath { get; }
        IEnumerable<FileInProject> Files { get; }
    }
}