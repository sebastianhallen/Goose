namespace Goose.Core.Solution
{
    using System.Collections.Generic;

    public interface ISolutionProject
    {
        string ProjectFilePath { get; }
        IEnumerable<ProjectFile> Files { get; }
    }
}