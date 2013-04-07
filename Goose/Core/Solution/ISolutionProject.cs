namespace Goose.Core.Solution
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Shell.Interop;

    public interface ISolutionProject
    {
        IVsHierarchy Hierarchy { get; }
        string SolutionFilePath { get; }
        string ProjectFilePath { get; }
        IEnumerable<FileInProject> Files { get; }
    }
}