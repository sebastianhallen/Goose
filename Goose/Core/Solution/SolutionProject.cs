﻿namespace Goose.Core.Solution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Debugging;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Output;

    public class SolutionProject 
        : ISolutionProject
    {
        private readonly IVsProject vsProject;
        private readonly IOutputService outputService;

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
                    this.outputService.Debug<SolutionProject>("project path empty in Files { get; }");
                    return Enumerable.Empty<FileInProject>();
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
                        return new FileInProject(projectPath, filePath, itemId);
                    })
                                .Where(file => !String.IsNullOrEmpty(file.ProjectPath))
                                .Where(file => File.Exists(file.FilePath));
            }
        }

        public SolutionProject(IVsProject vsProject, IOutputService outputService)
        {
            this.vsProject = vsProject;
            this.outputService = outputService;
        }
    }
}