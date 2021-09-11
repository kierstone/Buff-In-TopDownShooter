using Codice.Client.BaseCommands.Merge;
using PlasticGui.WorkspaceWindow.IncomingChanges;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer
{
    // internal for testing purpuses
    internal static class IsCurrent
    {
        internal static bool Conflict(
            IncomingChangeInfo changeInfo,
            IncomingChangeInfo metaChangeInfo,
            MergeSolvedFileConflicts solvedFileConflicts)
        {
            if (solvedFileConflicts == null)
                return false;

            MergeSolvedFileConflicts.CurrentConflict currentConflict;

            if (!solvedFileConflicts.TryGetCurrentConflict(out currentConflict))
                return false;

            return IsSameConflict(currentConflict, changeInfo) ||
                   IsSameConflict(currentConflict, metaChangeInfo);
        }

        static bool IsSameConflict(
            MergeSolvedFileConflicts.CurrentConflict currentConflict,
            IncomingChangeInfo changeInfo)
        {
            if (changeInfo == null)
                return false;

            return currentConflict.MountId.Equals(changeInfo.GetMount().Id) &&
                   currentConflict.ItemId == changeInfo.GetRevision().ItemId;
        }
    }
}
