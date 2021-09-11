using UnityEditor.IMGUI.Controls;

using PlasticGui.WorkspaceWindow.IncomingChanges;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer
{
    internal class ChangeCategoryTreeViewItem : TreeViewItem
    {
        internal IncomingChangesCategory Category { get; private set; }

        internal ChangeCategoryTreeViewItem(int id, IncomingChangesCategory category)
            : base(id, 0, category.CategoryType.ToString())
        {
            Category = category;
        }
    }
}
