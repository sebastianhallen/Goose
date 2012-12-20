namespace tretton37.RunCommandOnSave.LessAutoCompiler
{
	using System.Collections.Generic;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;

	public static class HierarchyTraversionExtensions
    {
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
            return GetReleative(hierarchy, rootItemId, __VSHPROPID.VSHPROPID_FirstChild);   
        }

        public static uint NextSibling(this IVsHierarchy hierarchy, uint previousSiblingItemId)
        {
            return GetReleative(hierarchy, previousSiblingItemId, __VSHPROPID.VSHPROPID_NextSibling);         
        }

        private static uint GetReleative(this IVsHierarchy hierarchy, uint referenceItemId, __VSHPROPID relativeType)
        {
            object relative;
            hierarchy.GetProperty(referenceItemId, (int) relativeType, out relative);
            return relative == null ? (uint)VSConstants.VSITEMID.Nil : (uint)(int)relative;
        }
        
    }
}