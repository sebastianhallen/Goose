namespace Goose.Core.Solution
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class SolutionProject 
        : ISolutionProject
    {
        private IVsSolution vsSolution;
        private readonly IVsProject vsProject;

        public IVsHierarchy Hierarchy
        {
            get { return this.vsProject as IVsHierarchy; }
        }

        public string SolutionFilePath
        {
            get
            {
                string solutionDirectory;
                string solutionFile;
                string userOptionsFile;
                this.vsSolution.GetSolutionInfo(out solutionDirectory, out solutionFile, out userOptionsFile);

                return solutionFile;
            }
        }

        public string ProjectFilePath
        {
            get
            {
                var path = "";
                this.vsProject.GetMkDocument((uint)VSConstants.VSITEMID.Root, out path);
                return path;
            }
        }

        public IEnumerable<FileInProject> Files
        {
            get
            {
                var projectPath = this.ProjectFilePath;
                if ( string.IsNullOrWhiteSpace( projectPath ) )
                {                    
                    return Enumerable.Empty<FileInProject>();
                }
                
                return this.Hierarchy.GetItemIds().Select(itemId =>
                    {
                        string filePath = null;
                        
                        this.vsProject.GetMkDocument(itemId, out filePath);
                        if (!String.IsNullOrEmpty(filePath) && !Path.IsPathRooted(filePath))
                        {
                            filePath = Path.GetFullPath(Path.Combine(projectPath, filePath));
                        }
                        return new FileInProject(projectPath, filePath, itemId);
                    })
                    .Where(file => !String.IsNullOrEmpty(file.FilePath))
                    .Where(file => !String.IsNullOrEmpty(file.ProjectPath))
                    .Where(file => File.Exists(file.FilePath));
            }
        }

        public SolutionProject(IVsSolution vsSolution, IVsProject vsProject)
        {
            this.vsSolution = vsSolution;
            this.vsProject = vsProject;            
        }
    }
}