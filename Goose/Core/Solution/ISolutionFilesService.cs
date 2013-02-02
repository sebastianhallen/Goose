namespace Goose.Core.Solution
{
    using System.Collections.Generic;

    public interface ISolutionFilesService
    {
        IEnumerable<ISolutionProject> Projects { get; }
    }
}