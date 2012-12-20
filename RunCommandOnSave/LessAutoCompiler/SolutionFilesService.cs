namespace tretton37.RunCommandOnSave.LessAutoCompiler
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Microsoft.VisualStudio;
  using Microsoft.VisualStudio.Shell.Interop;

  public class SolutionFilesService
    {
        private readonly IServiceProvider serviceProvider;
        private IVsSolution Solution
        {
            get
            {
                return this.serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            }
        }

        public SolutionFilesService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;            
        }

        public IEnumerable<SolutionProject> Projects
        {
            get
            {
                return ExtractProjectsFromSolution().Select(project => new SolutionProject(project));
            }            
            
        }

        private IEnumerable<IVsProject> ExtractProjectsFromSolution()
        {
            IEnumHierarchies enumerator;
            var guid = Guid.Empty;
            this.Solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref guid, out enumerator);

            var hierarchy = new IVsHierarchy[] { null };
            uint fetched;
            for (enumerator.Reset(); enumerator.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1; )
            {
                yield return (IVsProject)hierarchy[0];
            }
        }
    }
}
