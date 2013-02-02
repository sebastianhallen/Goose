namespace Goose.Core.Solution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    public class SolutionProject 
        : ISolutionProject
    {
        private readonly IVsProject vsProject;

        public string ProjectFilePath
        {
            get
            {
                var path = "";
                this.vsProject.GetMkDocument((uint)VSConstants.VSITEMID.Root, out path);
                return path;
            }
        }

        public IEnumerable<ProjectFile> Files
        {
            get
            {
                var projectPath = this.ProjectFilePath;
                if ( string.IsNullOrWhiteSpace( projectPath ) )
                {
                    return Enumerable.Empty<ProjectFile>();
                }
                var hierarchy = this.vsProject as IVsHierarchy;

                return hierarchy.GetItemIds().Select(itemId =>
                    {
                        string filePath = null;
                        this.vsProject.GetMkDocument(itemId, out filePath);
                        if (!String.IsNullOrEmpty(filePath) && !Path.IsPathRooted(filePath))
                        {
                            filePath = Path.GetFullPath(Path.Combine(projectPath, filePath));
                        }
                        return new ProjectFile(this.ProjectFilePath, filePath, itemId);
                    })
                                .Where(file => !String.IsNullOrEmpty(file.ProjectPath))
                                .Where(file => File.Exists(file.FilePath));
            }
        }

        public SolutionProject(IVsProject vsProject)
        {
            this.vsProject = vsProject;
        }
    }
}