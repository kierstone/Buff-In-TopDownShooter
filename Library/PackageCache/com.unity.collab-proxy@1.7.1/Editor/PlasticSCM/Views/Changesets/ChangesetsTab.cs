using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.Client.Commands;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.QueryViews;
using PlasticGui.WorkspaceWindow.QueryViews.Changesets;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.Views.Diff;

namespace Unity.PlasticSCM.Editor.Views.Changesets
{
    internal class ChangesetsTab :
        IRefreshableView,
        IChangesetMenuOperations,
        ChangesetsViewMenu.IMenuOperations
    {
        internal ChangesetsTab(
            WorkspaceInfo wkInfo,
            IWorkspaceWindow workspaceWindow,
            IViewSwitcher viewSwitcher,
            IHistoryViewLauncher historyViewLauncher,
            EditorWindow parentWindow,
            bool isGluonMode)
        {
            mWkInfo = wkInfo;
            mParentWindow = parentWindow;
            mIsGluonMode = isGluonMode;

            BuildComponents(
                wkInfo, workspaceWindow, viewSwitcher,
                historyViewLauncher, parentWindow);

            mProgressControls = new ProgressControlsForViews();

            mSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.50f, 0.50f },
                new int[] { 100, 100 },
                new int[] { 100000, 100000 }
            );

            ((IRefreshableView)this).Refresh();
        }

        internal void OnDisable()
        {
            mDiffPanel.OnDisable();

            mSearchField.downOrUpArrowKeyPressed -=
                SearchField_OnDownOrUpArrowKeyPressed;

            TreeHeaderSettings.Save(
                mChangesetsListView.multiColumnHeader.state,
                UnityConstants.CHANGESETS_TABLE_SETTINGS_NAME);

            BoolSetting.Save(
                mIsChangesPanelVisible,
                UnityConstants.CHANGESETS_SHOW_CHANGES_SETTING_NAME);
        }

        internal void Update()
        {
            mDiffPanel.Update();

            mProgressControls.UpdateProgress(mParentWindow);
        }

        internal void OnGUI()
        {
            InitializeShowChangesButtonWidth();

            bool wasChangesPanelVisible = mIsChangesPanelVisible;

            DoActionsToolbar(
                this,
                mProgressControls,
                mSearchField,
                mChangesetsListView,
                mDateFilter,
                mChangesetsLabelText,
                mShowChangesButtonWidth,
                wasChangesPanelVisible);

            if (mIsChangesPanelVisible)
            {
                PlasticSplitterGUILayout.BeginVerticalSplit(mSplitterState);
            }

            DoChangesetsArea(
                mChangesetsListView,
                mProgressControls.IsOperationRunning());

            if (mIsChangesPanelVisible)
            {
                if (!wasChangesPanelVisible)
                    mShouldScrollToSelection = true;

                DoChangesArea(mDiffPanel);
                PlasticSplitterGUILayout.EndVerticalSplit();
            }
        }

        void IRefreshableView.Refresh()
        {
            string query = GetChangesetsQuery(mDateFilter);

            FillChangesets(mWkInfo, query);
        }

        int IChangesetMenuOperations.GetSelectedChangesetsCount()
        {
            return ChangesetsSelection.GetSelectedChangesetsCount(mChangesetsListView);
        }

        void IChangesetMenuOperations.DiffChangeset()
        {
            LaunchDiffOperations.DiffChangeset(
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                ChangesetsSelection.GetSelectedChangeset(mChangesetsListView),
                mIsGluonMode);
        }

        void IChangesetMenuOperations.DiffSelectedChangesets()
        {
            List<RepObjectInfo> selectedChangesets = ChangesetsSelection.
                GetSelectedRepObjectInfos(mChangesetsListView);

            if (selectedChangesets.Count < 2)
                return;

            LaunchDiffOperations.DiffSelectedChangesets(
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                (ChangesetExtendedInfo)selectedChangesets[0],
                (ChangesetExtendedInfo)selectedChangesets[1],
                mIsGluonMode);
        }

        void IChangesetMenuOperations.DiffWithAnotherChangeset() { }
        void IChangesetMenuOperations.CreateBranch() { }
        void IChangesetMenuOperations.LabelChangeset() { }
        void IChangesetMenuOperations.SwitchToChangeset() { }
        void IChangesetMenuOperations.MergeChangeset() {}
        void IChangesetMenuOperations.CherryPickChangeset() { }
        void IChangesetMenuOperations.SubtractiveChangeset() { }
        void IChangesetMenuOperations.SubtractiveChangesetInterval() { }
        void IChangesetMenuOperations.CherryPickChangesetInterval() { }
        void IChangesetMenuOperations.MergeToChangeset() { }
        void IChangesetMenuOperations.MoveChangeset() { }
        void IChangesetMenuOperations.DeleteChangeset() { }
        void IChangesetMenuOperations.BrowseRepositoryOnChangeset() { }
        void IChangesetMenuOperations.CreateCodeReview() { }

        void SearchField_OnDownOrUpArrowKeyPressed()
        {
            mChangesetsListView.SetFocusAndEnsureSelectedItem();
        }

        void FillChangesets(WorkspaceInfo wkInfo, string query)
        {
            if (mIsRefreshing)
                return;

            mIsRefreshing = true;

            List<RepObjectInfo> changesetsToSelect =
                ChangesetsSelection.GetSelectedRepObjectInfos(mChangesetsListView);

            int defaultRow = TableViewOperations.
                GetFirstSelectedRow(mChangesetsListView);

            ((IProgressControls)mProgressControls).ShowProgress(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.LoadingChangesets));

            long loadedChangesetId = -1;
            ViewQueryResult queryResult = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter();
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    queryResult = new ViewQueryResult(
                        Plastic.API.FindQuery(wkInfo, query));

                    loadedChangesetId = GetLoadedChangesetId(
                        wkInfo, mIsGluonMode);
                },
                /*afterOperationDelegate*/ delegate
                {
                    try
                    {
                        if (waiter.Exception != null)
                        {
                            ExceptionsHandler.DisplayException(waiter.Exception);
                            return;
                        }

                        int changesetsCount = GetChangesetsCount(queryResult);

                        mChangesetsLabelText = string.Format(
                            PlasticLocalization.GetString(PlasticLocalization.Name.ChangesetsCount),
                            changesetsCount);

                        UpdateChangesetsList(
                            mChangesetsListView,
                            queryResult,
                            loadedChangesetId);

                        if (changesetsCount == 0)
                        {
                            mDiffPanel.ClearInfo();
                            return;
                        }

                        ChangesetsSelection.SelectChangesets(
                            mChangesetsListView, changesetsToSelect, defaultRow);
                    }
                    finally
                    {
                        ((IProgressControls)mProgressControls).HideProgress();
                        mIsRefreshing = false;
                    }
                });
        }

        void ChangesetsViewMenu.IMenuOperations.DiffBranch()
        {
            LaunchDiffOperations.DiffBranch(
                ChangesetsSelection.GetSelectedRepository(mChangesetsListView),
                ChangesetsSelection.GetSelectedChangeset(mChangesetsListView),
                mIsGluonMode);
        }

        ChangesetExtendedInfo ChangesetsViewMenu.IMenuOperations.GetSelectedChangeset()
        {
            return ChangesetsSelection.GetSelectedChangeset(
                mChangesetsListView);
        }

        void OnChangesetsListViewSizeChanged()
        {
            if (!mShouldScrollToSelection)
                return;

            mShouldScrollToSelection = false;
            TableViewOperations.ScrollToSelection(mChangesetsListView);
        }

        void OnSelectionChanged()
        {
            List<RepObjectInfo> selectedChangesets = ChangesetsSelection.
                GetSelectedRepObjectInfos(mChangesetsListView);

            if (selectedChangesets.Count != 1)
                return;

            mDiffPanel.UpdateInfo(
                MountPointWithPath.BuildWorkspaceRootMountPoint(
                    ChangesetsSelection.GetSelectedRepository(mChangesetsListView)),
                (ChangesetExtendedInfo)selectedChangesets[0]);
        }

        static void UpdateChangesetsList(
            ChangesetsListView changesetsListView,
            ViewQueryResult queryResult,
            long loadedChangesetId)
        {
            changesetsListView.BuildModel(
                queryResult,
                loadedChangesetId);

            changesetsListView.Refilter();

            changesetsListView.Sort();

            changesetsListView.Reload();
        }

        static long GetLoadedChangesetId(
            WorkspaceInfo wkInfo,
            bool isGluonMode)
        {
            if (isGluonMode)
                return -1;

            return Plastic.API.GetLoadedChangeset(wkInfo);
        }

        static string GetChangesetsQuery(DateFilter dateFilter)
        {
            if (dateFilter.FilterType == DateFilter.Type.AllTime)
                return QueryConstants.ChangesetsBeginningQuery;

            string whereClause = QueryConstants.GetChangesetsDateWhereClause(
                dateFilter.GetFilterDate(DateTime.UtcNow));

            return string.Format("{0} {1}",
                QueryConstants.ChangesetsBeginningQuery,
                whereClause);
        }

        static int GetChangesetsCount(
            ViewQueryResult queryResult)
        {
            if (queryResult == null)
                return 0;

            return queryResult.Count();
        }

        void DoActionsToolbar(
            IRefreshableView refreshableView,
            ProgressControlsForViews progressControls,
            SearchField searchField,
            ChangesetsListView changesetsListView,
            DateFilter dateFilter,
            string changesetsLabelText,
            float showChangesButtonWidth,
            bool wasChangesPanelVisible)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (!string.IsNullOrEmpty(changesetsLabelText))
            {
                GUILayout.Label(
                    changesetsLabelText,
                    UnityStyles.ChangesetsTab.HeaderLabel);
            }

            if (progressControls.IsOperationRunning())
            {
                DrawProgressForViews.ForIndeterminateProgress(
                    progressControls.ProgressData);
            }

            GUILayout.FlexibleSpace();

            mIsChangesPanelVisible =
                DoShowChangesButton(
                    showChangesButtonWidth,
                    wasChangesPanelVisible);

            GUILayout.Space(2);

            DrawSearchField.For(
                searchField,
                changesetsListView,
                UnityConstants.SEARCH_FIELD_WIDTH);

            DoDateFilter(
                refreshableView,
                dateFilter,
                progressControls.IsOperationRunning());

            DoRefreshButton(
                refreshableView,
                progressControls.IsOperationRunning());

            EditorGUILayout.EndHorizontal();
        }

        static void DoChangesetsArea(
            ChangesetsListView changesetsListView,
            bool isOperationRunning)
        {
            EditorGUILayout.BeginVertical();

            GUI.enabled = !isOperationRunning;

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            changesetsListView.OnGUI(rect);

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }

        static void DoChangesArea(DiffPanel diffPanel)
        {
            EditorGUILayout.BeginVertical();

            diffPanel.OnGUI();

            EditorGUILayout.EndVertical();
        }

        static bool DoShowChangesButton(
            float showChangesButtonWidth,
            bool wasChecked)
        {
            string buttonText = wasChecked ?
                PlasticLocalization.GetString(PlasticLocalization.Name.HideChanges) :
                PlasticLocalization.GetString(PlasticLocalization.Name.ShowChanges);

            GUIContent buttonContent = new GUIContent(buttonText);

            GUIStyle buttonStyle = EditorStyles.toolbarButton;

            Rect toggleRect = GUILayoutUtility.GetRect(
                buttonContent, buttonStyle,
                GUILayout.Width(showChangesButtonWidth));

            bool isChecked = GUI.Toggle(
                toggleRect, wasChecked, buttonContent, buttonStyle);

            return isChecked;
        }

        static void DoDateFilter(
            IRefreshableView refreshableView,
            DateFilter dateFilter,
            bool isOperationRunning)
        {
            GUI.enabled = !isOperationRunning;

            EditorGUI.BeginChangeCheck();

            dateFilter.FilterType = (DateFilter.Type)
                EditorGUILayout.EnumPopup(
                    dateFilter.FilterType,
                    EditorStyles.toolbarDropDown,
                    GUILayout.Width(100));

            if (EditorGUI.EndChangeCheck())
            {
                EnumPopupSetting<DateFilter.Type>.Save(
                    dateFilter.FilterType,
                    UnityConstants.CHANGESETS_DATE_FILTER_SETTING_NAME);

                refreshableView.Refresh();
            }

            GUI.enabled = true;
        }

        static void DoRefreshButton(
            IRefreshableView refreshableView,
            bool isOperationRunning)
        {
            EditorGUI.BeginDisabledGroup(isOperationRunning);

            if (GUILayout.Button(new GUIContent(
                    Images.GetRefreshIcon()), EditorStyles.toolbarButton))
                refreshableView.Refresh();

            EditorGUI.EndDisabledGroup();
        }

        void InitializeShowChangesButtonWidth()
        {
            if (mShowChangesButtonWidth != -1)
                return;

            mShowChangesButtonWidth = MeasureMaxWidth.ForTexts(
                EditorStyles.toolbarButton,
                PlasticLocalization.GetString(PlasticLocalization.Name.HideChanges),
                PlasticLocalization.GetString(PlasticLocalization.Name.ShowChanges));
        }

        void BuildComponents(
            WorkspaceInfo wkInfo,
            IWorkspaceWindow workspaceWindow,
            IViewSwitcher viewSwitcher,
            IHistoryViewLauncher historyViewLauncher,
            EditorWindow parentWindow)
        {
            mSearchField = new SearchField();
            mSearchField.downOrUpArrowKeyPressed += SearchField_OnDownOrUpArrowKeyPressed;

            DateFilter.Type dateFilterType =
                EnumPopupSetting<DateFilter.Type>.Load(
                    UnityConstants.CHANGESETS_DATE_FILTER_SETTING_NAME,
                    DateFilter.Type.LastMonth);
            mDateFilter = new DateFilter(dateFilterType);

            ChangesetsListHeaderState headerState =
                ChangesetsListHeaderState.GetDefault();
            TreeHeaderSettings.Load(headerState,
                UnityConstants.CHANGESETS_TABLE_SETTINGS_NAME,
                (int)ChangesetsListColumn.CreationDate, false);

            mChangesetsListView = new ChangesetsListView(
                headerState,
                ChangesetsListHeaderState.GetColumnNames(),
                new ChangesetsViewMenu(this, this, mIsGluonMode),
                sizeChangedAction: OnChangesetsListViewSizeChanged,
                selectionChangedAction: OnSelectionChanged,
                doubleClickAction: ((IChangesetMenuOperations)this).DiffChangeset);
            mChangesetsListView.Reload();

            mIsChangesPanelVisible = BoolSetting.Load(
                UnityConstants.CHANGESETS_SHOW_CHANGES_SETTING_NAME,
                true);

            mDiffPanel = new DiffPanel(
                wkInfo, workspaceWindow, viewSwitcher,
                historyViewLauncher, parentWindow, mIsGluonMode);
        }

        bool mIsRefreshing;

        bool mShouldScrollToSelection;

        bool mIsChangesPanelVisible;

        float mShowChangesButtonWidth = -1;

        string mChangesetsLabelText;
        object mSplitterState;

        DateFilter mDateFilter;

        SearchField mSearchField;
        ChangesetsListView mChangesetsListView;
        DiffPanel mDiffPanel;

        readonly bool mIsGluonMode;

        readonly ProgressControlsForViews mProgressControls;
        readonly EditorWindow mParentWindow;
        readonly WorkspaceInfo mWkInfo;
    }
}