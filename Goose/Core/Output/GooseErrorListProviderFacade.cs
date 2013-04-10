namespace Goose.Core.Output
{
    using System;
    using System.Linq;
    using Goose.Core.Solution;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    public class GooseErrorListProviderFacade
        : ErrorListProvider, IErrorListProviderFacade
    {
        private readonly ISolutionFilesService solutionFiles;

        public GooseErrorListProviderFacade(IServiceProvider provider, ISolutionFilesService solutionFiles) 
            : base(provider)
        {
            this.solutionFiles = solutionFiles;
        }

        public void Add(IGooseErrorTask error)
        {
            error.Error.HierarchyItem = this.FindHierarchyItem(error.Error.Document);
            error.Error.Navigate += (s, a) =>
                {
                    var task = (ErrorTask) s;
                    this.Navigate(task, new Guid(EnvDTE.Constants.vsViewKindCode));
                };
            this.Tasks.Add(error.Error);
        }

        public void Remove(IGooseErrorTask error)
        {
            this.Tasks.Remove(error.Error);
        }

        private IVsHierarchy FindHierarchyItem(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;

            var project = this.solutionFiles.Projects
                                .FirstOrDefault(prj => prj.Files
                                    .Any(file => file.FilePath.Equals(filePath)));

            return project == null
                       ? null
                       : project.Hierarchy;
        }
    }
}