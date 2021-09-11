using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI
{
    internal static class DrawSearchField
    {
        internal static void For(
            SearchField searchField,
            TreeView treeView,
            float width)
        {
            Rect searchFieldRect = GUILayoutUtility.GetRect(
                width / 2, EditorGUIUtility.singleLineHeight);
            searchFieldRect.y += 2;

            treeView.searchString = searchField.OnToolbarGUI(
                searchFieldRect, treeView.searchString);

            if (!string.IsNullOrEmpty(treeView.searchString))
                return;

            GUI.Label(searchFieldRect, PlasticLocalization.GetString(
                PlasticLocalization.Name.SearchTooltip), UnityStyles.Search);
        }
    }
}
