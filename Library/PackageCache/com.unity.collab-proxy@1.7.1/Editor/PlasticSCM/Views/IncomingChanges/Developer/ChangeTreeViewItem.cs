using UnityEditor.IMGUI.Controls;

using PlasticGui.WorkspaceWindow.IncomingChanges;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer
{
    internal class ChangeTreeViewItem : TreeViewItem
    {
        internal IncomingChangeInfo ChangeInfo { get; private set; }

        internal ChangeTreeViewItem(int id, IncomingChangeInfo change)
            : base(id, 1)
        {
            ChangeInfo = change;

            displayName = id.ToString();
        }
    }
}
