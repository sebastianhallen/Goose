namespace Goose.Core.Solution
{
    using System.Collections.Generic;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    public static class HierarchyTraversionExtensions
    {
        public static Project AsProject(this IVsHierarchy hierarchy)
        {
            object project;
            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out project);

            return project as Project;
        }

        public static IEnumerable<uint> GetItemIds(this IVsHierarchy hierarchy, uint rootItemId = (uint) VSConstants.VSITEMID.Root)
        {
            for (var itemId = hierarchy.FirstChild(rootItemId); itemId != (uint)VSConstants.VSITEMID.Nil; itemId = hierarchy.NextSibling(itemId))
            {
                yield return itemId;
                foreach (var childItemId in GetItemIds(hierarchy, itemId))
                {
                    yield return childItemId;
                }
            }
        }

        public static uint FirstChild(this IVsHierarchy hierarchy, uint rootItemId)
        {
            return GetReleativeItemId(hierarchy, rootItemId, __VSHPROPID.VSHPROPID_FirstChild);   
        }

        public static uint NextSibling(this IVsHierarchy hierarchy, uint previousSiblingItemId)
        {
            return GetReleativeItemId(hierarchy, previousSiblingItemId, __VSHPROPID.VSHPROPID_NextSibling);         
        }

        private static uint GetReleativeItemId(this IVsHierarchy hierarchy, uint referenceItemId, __VSHPROPID relativeType)
        {
            object relative;
            hierarchy.GetProperty(referenceItemId, (int) relativeType, out relative);
            return relative == null ? (uint)VSConstants.VSITEMID.Nil : (uint)(int)relative;
        }
        
    }
}