namespace tretton37.RunCommandOnSave.LessAutoCompiler
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;  
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell.Interop;

  public class SolutionProject
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
                var hierarchy = this.vsProject as IVsHierarchy;

                return hierarchy.GetItemIds().Select(itemId =>
                    {
                        string filePath = null;
                        this.vsProject.GetMkDocument(itemId, out filePath);
                        if (!String.IsNullOrEmpty(filePath) && !Path.IsPathRooted(filePath))
                        {
                            filePath = Path.GetFullPath(Path.Combine(this.ProjectFilePath, filePath));
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