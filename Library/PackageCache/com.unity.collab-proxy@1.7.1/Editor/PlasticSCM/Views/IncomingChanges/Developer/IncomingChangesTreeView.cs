using System.Collections.Generic;
using System.IO;

using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.Client.BaseCommands.Merge;
using Codice.Client.Common;
using Codice.CM.Common;
using PlasticGui.WorkspaceWindow.IncomingChanges;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Tree;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer
{
    internal class IncomingChangesTreeView : TreeView
    {
        internal IncomingChangesTreeView(
            WorkspaceInfo wkInfo,
            IncomingChangesTreeHeaderState headerState,
            List<string> columnNames,
            IncomingChangesViewMenu menu)
            : base(new TreeViewState())
        {
            mWkInfo = wkInfo;
            mColumnNames = columnNames;
            mMenu = menu;

            multiColumnHeader = new MultiColumnHeader(headerState);
            multiColumnHeader.canSort = true;
            multiColumnHeader.sortingChanged += SortingChanged;

            customFoldoutYOffset = UnityConstants.TREEVIEW_FOLDOUT_Y_OFFSET;
            rowHeight = UnityConstants.TREEVIEW_ROW_HEIGHT;
            showAlternatingRowBackgrounds = true;
        }

        public override IList<TreeViewItem> GetRows()
        {
            return mRows;
        }

        protected override bool CanChangeExpandedState(TreeViewItem item)
        {
            return item is ChangeCategoryTreeViewItem;
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem(0, -1, string.Empty);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem rootItem)
        {
            try
            {
                RegenerateRows(
                    mIncomingChangesTree,
                    mTreeViewItemIds,
                    this,
                    rootItem,
                    mRows,
                    mExpandCategories);
            }
            finally
            {
                mExpandCategories = false;
            }

            return mRows;
        }

        protected override void CommandEventHandling()
        {
            // NOTE - empty override to prevent crash when pressing ctrl-a in the treeview
        }

        protected override void ContextClickedItem(int id)
        {
            mMenu.Popup();
            Repaint();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            DrawTreeViewItem.InitializeStyles();

            if (args.item is ChangeCategoryTreeViewItem)
            {
                ChangeCategoryTreeViewItem categoryItem =
                    (ChangeCategoryTreeViewItem)args.item;

                CategoryTreeViewItemGUI(
                    args.rowRect, rowHeight,
                    categoryItem,
                    GetSolvedChildrenCount(categoryItem.Category, mSolvedFileConflicts),
                    args.selected,
                    args.focused);
                return;
            }

            if (args.item is ChangeTreeViewItem)
            {
                ChangeTreeViewItem changeTreeViewItem =
                    (ChangeTreeViewItem)args.item;

                IncomingChangeInfo changeInfo =
                    changeTreeViewItem.ChangeInfo;

                bool isCurrentConflict = IsCurrent.Conflict(
                    changeInfo,
                    mIncomingChangesTree.GetMetaChange(changeInfo),
                    mSolvedFileConflicts);

                bool isSolvedConflict = IsSolved.Conflict(
                    changeInfo,
                    mIncomingChangesTree.GetMetaChange(changeInfo),
                    mSolvedFileConflicts);

                IncomingChangeTreeViewItemGUI(
                    mWkInfo.ClientPath,
                    mIncomingChangesTree,
                    this,
                    changeTreeViewItem,
                    args,
                    isCurrentConflict,
                    isSolvedConflict);
                return;
            }

            base.RowGUI(args);
        }

        internal void SelectFirstUnsolvedDirectoryConflict()
        {
            foreach (IncomingChangesCategory category in mIncomingChangesTree.GetNodes())
            {
                if (category.CategoryType != IncomingChangesCategory.Type.DirectoryConflicts)
                    continue;

                foreach (IncomingChangeInfo changeInfo in category.GetChanges())
                {
                    if (changeInfo.DirectoryConflict.IsResolved())
                        continue;

                    int itemId = -1;
                    if (mTreeViewItemIds.TryGetInfoItemId(changeInfo, out itemId))
                    {
                        SetSelection(new List<int>() { itemId });
                        return;
                    }
                }
            }
        }

        internal void BuildModel(UnityIncomingChangesTree tree)
        {
            mTreeViewItemIds.Clear();

            mIncomingChangesTree = tree;
            mSolvedFileConflicts = null;

            mExpandCategories = true;
        }

        internal void Sort()
        {
            int sortedColumnIdx = multiColumnHeader.state.sortedColumnIndex;
            bool sortAscending = multiColumnHeader.IsSortedAscending(sortedColumnIdx);

            mIncomingChangesTree.Sort(mColumnNames[sortedColumnIdx], sortAscending);
        }

        internal void UpdateSolvedFileConflicts(
            MergeSolvedFileConflicts solvedFileConflicts)
        {
            mSolvedFileConflicts = solvedFileConflicts;
        }

        internal IncomingChangeInfo GetMetaChange(IncomingChangeInfo change)
        {
            if (change == null)
                return null;

            return mIncomingChangesTree.GetMetaChange(change);
        }

        internal void FillWithMeta(List<IncomingChangeInfo> changes)
        {
            mIncomingChangesTree.FillWithMeta(changes);
        }

        internal bool SelectionHasMeta()
        {
            IncomingChangeInfo selectedChangeInfo = GetSelectedIncomingChange();

            if (selectedChangeInfo == null)
                return false;

            return mIncomingChangesTree.HasMeta(selectedChangeInfo);
        }

        internal IncomingChangeInfo GetSelectedIncomingChange()
        {
            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count != 1)
                return null;

            int selectedId = selectedIds[0];

            foreach (KeyValuePair<IncomingChangeInfo, int> item
                in mTreeViewItemIds.GetInfoItems())
            {
                if (selectedId == item.Value)
                    return item.Key;
            }

            return null;
        }

        internal List<IncomingChangeInfo> GetSelectedIncomingChanges()
        {
            List<IncomingChangeInfo> result = new List<IncomingChangeInfo>();

            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count == 0)
                return result;

            foreach (KeyValuePair<IncomingChangeInfo, int> item
                in mTreeViewItemIds.GetInfoItems())
            {
                if (!selectedIds.Contains(item.Value))
                    continue;

                result.Add(item.Key);
            }

            return result;
        }

        internal List<IncomingChangeInfo> GetSelectedFileConflicts()
        {
            List<IncomingChangeInfo> result = new List<IncomingChangeInfo>();

            IList<int> selectedIds = GetSelection();

            if (selectedIds.Count == 0)
                return result;

            foreach (KeyValuePair<IncomingChangeInfo, int> item
                in mTreeViewItemIds.GetInfoItems())
            {
                if (!selectedIds.Contains(item.Value))
                    continue;

                if (item.Key.CategoryType !=
                        IncomingChangesCategory.Type.FileConflicts)
                    continue;

                result.Add(item.Key);
            }

            return result;
        }

        void SortingChanged(MultiColumnHeader multiColumnHeader)
        {
            Sort();

            Reload();
        }

        static void RegenerateRows(
            UnityIncomingChangesTree incomingChangesTree,
            TreeViewItemIds<IncomingChangesCategory, IncomingChangeInfo> treeViewItemIds,
            IncomingChangesTreeView treeView,
            TreeViewItem rootItem,
            List<TreeViewItem> rows,
            bool expandCategories)
        {
            if (incomingChangesTree == null)
                return;

            ClearRows(rootItem, rows);

            List<IncomingChangesCategory> categories = incomingChangesTree.GetNodes();

            if (categories == null)
                return;

            List<int> categoriesToExpand = new List<int>();

            foreach (IncomingChangesCategory category in categories)
            {
                int categoryId;
                if (!treeViewItemIds.TryGetCategoryItemId(category, out categoryId))
                    categoryId = treeViewItemIds.AddCategoryItem(category);

                ChangeCategoryTreeViewItem categoryTreeViewItem =
                    new ChangeCategoryTreeViewItem(categoryId, category);

                rootItem.AddChild(categoryTreeViewItem);
                rows.Add(categoryTreeViewItem);

                if (!ShouldExpandCategory(
                        treeView,
                        categoryTreeViewItem,
                        expandCategories,
                        categories.Count))
                    continue;

                categoriesToExpand.Add(categoryTreeViewItem.id);

                foreach (IncomingChangeInfo changeInfo in category.GetChanges())
                {
                    int differenceId;
                    if (!treeViewItemIds.TryGetInfoItemId(changeInfo, out differenceId))
                        differenceId = treeViewItemIds.AddInfoItem(changeInfo);

                    TreeViewItem changeTreeViewItem =
                        new ChangeTreeViewItem(differenceId, changeInfo);

                    categoryTreeViewItem.AddChild(changeTreeViewItem);
                    rows.Add(changeTreeViewItem);
                }
            }

            treeView.state.expandedIDs = categoriesToExpand;
        }

        static void ClearRows(
            TreeViewItem rootItem,
            List<TreeViewItem> rows)
        {
            if (rootItem.hasChildren)
                rootItem.children.Clear();

            rows.Clear();
        }

        static void CategoryTreeViewItemGUI(
            Rect rowRect,
            float rowHeight,
            ChangeCategoryTreeViewItem item,
            int solvedChildrenCount,
            bool isSelected,
            bool isFocused)
        {
            Texture icon = GetCategoryIcon(item.Category.CategoryType);
            string label = item.Category.GetHeaderText();

            DefaultStyles.label = GetCategoryStyle(
                item.Category,
                solvedChildrenCount,
                isSelected);

            DrawTreeViewItem.ForCategoryItem(
                rowRect,
                rowHeight,
                item.depth,
                icon,
                label,
                isSelected,
                isFocused);

            DefaultStyles.label = UnityStyles.Tree.Label;
        }

        static void IncomingChangeTreeViewItemGUI(
            string wkPath,
            UnityIncomingChangesTree incomingChangesTree,
            IncomingChangesTreeView treeView,
            ChangeTreeViewItem item,
            RowGUIArgs args,
            bool isCurrentConflict,
            bool isSolvedConflict)
        {
            for (int visibleColumnIdx = 0; visibleColumnIdx < args.GetNumVisibleColumns(); visibleColumnIdx++)
            {
                Rect cellRect = args.GetCellRect(visibleColumnIdx);

                IncomingChangesTreeColumn column =
                    (IncomingChangesTreeColumn)args.GetColumn(visibleColumnIdx);

                IncomingChangeTreeViewItemCellGUI(
                    wkPath,
                    cellRect,
                    treeView.rowHeight,
                    incomingChangesTree,
                    treeView,
                    item,
                    column,
                    args.selected,
                    args.focused,
                    isCurrentConflict,
                    isSolvedConflict);
            }
        }

        static void IncomingChangeTreeViewItemCellGUI(
            string wkPath,
            Rect rect,
            float rowHeight,
            UnityIncomingChangesTree incomingChangesTree,
            IncomingChangesTreeView treeView,
            ChangeTreeViewItem item,
            IncomingChangesTreeColumn column,
            bool isSelected,
            bool isFocused,
            bool isCurrentConflict,
            bool isSolvedConflict)
        {
            IncomingChangeInfo incomingChange = item.ChangeInfo;

            string label = incomingChange.GetColumnText(
                IncomingChangesTreeHeaderState.GetColumnName(column));

            if (column == IncomingChangesTreeColumn.Path)
            {
                if (incomingChangesTree.HasMeta(item.ChangeInfo))
                    label = string.Concat(label, UnityConstants.TREEVIEW_META_LABEL);

                Texture icon = GetIcon(wkPath, incomingChange);

                GetChangesOverlayIcon.Data overlayIconData =
                    GetChangesOverlayIcon.ForPlasticIncomingChange(
                        incomingChange, isSolvedConflict);

                DrawTreeViewItem.ForItemCell(
                    rect,
                    rowHeight,
                    item.depth,
                    icon,
                    overlayIconData,
                    label,
                    isSelected,
                    isFocused,
                    isCurrentConflict,
                    false);

                return;
            }

            if (column == IncomingChangesTreeColumn.Size)
            {
                DrawTreeViewItem.ForSecondaryLabelRightAligned(
                    rect, label, isSelected, isFocused, isCurrentConflict);
                return;
            }

            DrawTreeViewItem.ForSecondaryLabel(
                rect, label, isSelected, isFocused, isCurrentConflict);
        }

        static Texture GetCategoryIcon(
            IncomingChangesCategory.Type categoryType)
        {
            switch (categoryType)
            {
                case IncomingChangesCategory.Type.DirectoryConflicts:
                case IncomingChangesCategory.Type.FileConflicts:
                    return Images.GetImage(Images.Name.IconMergeConflict);
                case IncomingChangesCategory.Type.Changed:
                    return Images.GetImage(Images.Name.IconChanged);
                case IncomingChangesCategory.Type.Moved:
                    return Images.GetImage(Images.Name.IconMoved);
                case IncomingChangesCategory.Type.Deleted:
                    return Images.GetImage(Images.Name.IconDeleted);
                case IncomingChangesCategory.Type.Added:
                    return Images.GetImage(Images.Name.IconAdded);
                default:
                    return null;
            }
        }

        static Texture GetIcon(
            string wkPath,
            IncomingChangeInfo incomingChange)
        {
            RevisionInfo revInfo = incomingChange.GetRevision();
            bool isDirectory = revInfo.
                Type == EnumRevisionType.enDirectory;

            if (isDirectory || incomingChange.IsXLink())
                return Images.GetDirectoryIcon();

            string fullPath = WorkspacePath.GetWorkspacePathFromCmPath(
                wkPath,
                incomingChange.GetPath(),
                Path.DirectorySeparatorChar);

            return Images.GetFileIcon(fullPath);
        }

        static GUIStyle GetCategoryStyle(
            IncomingChangesCategory category,
            int solvedChildrenCount,
            bool isSelected)
        {
            if (isSelected)
                return UnityStyles.Tree.Label;

            if (category.CategoryType == IncomingChangesCategory.Type.FileConflicts ||
                category.CategoryType == IncomingChangesCategory.Type.DirectoryConflicts)
            {
                return category.GetChangesCount() > solvedChildrenCount ?
                    UnityStyles.Tree.RedLabel : UnityStyles.Tree.GreenLabel;
            }

            return UnityStyles.Tree.Label;
        }

        static bool ShouldExpandCategory(
            IncomingChangesTreeView treeView,
            ChangeCategoryTreeViewItem categoryTreeViewItem,
            bool expandCategories,
            int categoriesCount)
        {
            if (expandCategories)
            {
                if (categoriesCount == 1)
                    return true;

                if (categoryTreeViewItem.Category.CategoryType ==
                    IncomingChangesCategory.Type.FileConflicts)
                    return true;

                if (categoryTreeViewItem.Category.GetChangesCount() >
                    NODES_TO_EXPAND_CATEGORY)
                    return false;

                return true;
            }

            return treeView.IsExpanded(categoryTreeViewItem.id);
        }

        static int GetSolvedChildrenCount(
            IncomingChangesCategory category,
            MergeSolvedFileConflicts solvedFileConflicts)
        {
            int solvedDirConflicts = 0;
            if (category.CategoryType == IncomingChangesCategory.Type.DirectoryConflicts)
            {
                foreach (IncomingChangeInfo change in category.GetChanges())
                {
                    if (change.DirectoryConflict.IsResolved())
                        solvedDirConflicts++;
                }

                return solvedDirConflicts;
            }

            return (solvedFileConflicts == null) ? 0 :
                solvedFileConflicts.GetCount();
        }

        bool mExpandCategories;

        TreeViewItemIds<IncomingChangesCategory, IncomingChangeInfo> mTreeViewItemIds =
            new TreeViewItemIds<IncomingChangesCategory, IncomingChangeInfo>();
        List<TreeViewItem> mRows = new List<TreeViewItem>();

        MergeSolvedFileConflicts mSolvedFileConflicts;
        UnityIncomingChangesTree mIncomingChangesTree;

        readonly IncomingChangesViewMenu mMenu;
        readonly List<string> mColumnNames;
        readonly WorkspaceInfo mWkInfo;

        const int NODES_TO_EXPAND_CATEGORY = 10;
    }
}
