using System.Collections.Generic;

using PlasticGui.WorkspaceWindow.IncomingChanges;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer
{
    internal static class IncomingChangesSelection
    {
        internal static List<string> GetPathsFromSelectedFileConflictsIncludingMeta(
            IncomingChangesTreeView treeView)
        {
            List<string> result = new List<string>();

            List<IncomingChangeInfo> selection =
                treeView.GetSelectedFileConflicts();

            treeView.FillWithMeta(selection);

            foreach (IncomingChangeInfo incomingChange in selection)
            {
                result.Add(incomingChange.GetPath());
            }

            return result;
        }

        internal static SelectedIncomingChangesGroupInfo GetSelectedGroupInfo(
            IncomingChangesTreeView treeView)
        {
            List<IncomingChangeInfo> selectedIncomingChanges =
                treeView.GetSelectedIncomingChanges();

            return GetSelectedIncomingChangesGroupInfo.For(
                selectedIncomingChanges);
        }

        internal static IncomingChangeInfo GetSingleSelectedIncomingChange(
            IncomingChangesTreeView treeView)
        {
            return treeView.GetSelectedIncomingChange();
        }
    }
}
