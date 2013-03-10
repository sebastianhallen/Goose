namespace Goose.Core.Solution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Output;

    public class SolutionFilesService 
        : ISolutionFilesService
    {
        private readonly IOutputService outputService;
        private readonly IVsSolution solution;


        public SolutionFilesService(IVsSolution solution, IOutputService outputService)
        {
            this.solution = solution;
            this.outputService = outputService;
        }

        public IEnumerable<ISolutionProject> Projects
        {
            get
            {
                return ExtractProjectsFromSolution().Select(project => new SolutionProject(project, this.outputService)).ToArray();
            }            
            
        }

        private IEnumerable<IVsProject> ExtractProjectsFromSolution()
        {
            IEnumHierarchies enumerator;
            var guid = Guid.Empty;
            this.solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, ref guid, out enumerator);

            var hierarchy = new IVsHierarchy[] { null };
            uint fetched;
            for (enumerator.Reset(); enumerator.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1; )
            {
                var project = hierarchy[0] as IVsProject;
                if (project != null)
                {
                    yield return project;
                }
            }
        }
    }
}
