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
        private readonly IServiceProvider serviceProvider;
        private readonly IOutputService outputService;

        private IVsSolution Solution
        {
            get
            {
                return this.serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            }
        }

        public SolutionFilesService(IServiceProvider serviceProvider, IOutputService outputService)
        {
            this.serviceProvider = serviceProvider;
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
            this.Solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, ref guid, out enumerator);

            var hierarchy = new IVsHierarchy[] { null };
            uint fetched;
            for (enumerator.Reset(); enumerator.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1; )
            {
                yield return (IVsProject)hierarchy[0];
            }
        }
    }
}
