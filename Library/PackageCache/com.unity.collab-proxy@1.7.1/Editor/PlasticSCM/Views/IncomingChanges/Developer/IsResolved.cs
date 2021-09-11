using Codice.Client.BaseCommands.Merge;
using PlasticGui.WorkspaceWindow.IncomingChanges;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer
{
    internal static class IsSolved
    {
        internal static bool Conflict(
            IncomingChangeInfo changeInfo,
            IncomingChangeInfo metaChangeInfo,
            MergeSolvedFileConflicts solvedFileConflicts)
        {
            if (IsDirectoryConflict(changeInfo))
            {
                if (metaChangeInfo == null)
                    return IsDirectoryConflictResolved(changeInfo);

                return IsDirectoryConflictResolved(changeInfo) &&
                       IsDirectoryConflictResolved(metaChangeInfo);
            }

            if (metaChangeInfo == null)
            {
                return IsFileConflictResolved(
                    changeInfo, solvedFileConflicts);
            }

            return IsFileConflictResolved(changeInfo, solvedFileConflicts) && 
                   IsFileConflictResolved(metaChangeInfo, solvedFileConflicts);
        }

        static bool IsFileConflictResolved(
            IncomingChangeInfo changeInfo,
            MergeSolvedFileConflicts solvedFileConflicts)
        {
            if (solvedFileConflicts == null)
                return false;

            return solvedFileConflicts.IsResolved(
                changeInfo.GetMount().Id,
                changeInfo.GetRevision().ItemId);
        }

        static bool IsDirectoryConflictResolved(IncomingChangeInfo changeInfo)
        {
            return changeInfo.DirectoryConflict.IsResolved();
        }

        static bool IsDirectoryConflict(IncomingChangeInfo changeInfo)
        {
            return (changeInfo.DirectoryConflict != null);
        }
    }
}
