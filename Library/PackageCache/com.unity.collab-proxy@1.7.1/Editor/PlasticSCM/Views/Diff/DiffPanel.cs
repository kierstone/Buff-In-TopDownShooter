using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.Client.Commands;
using Codice.Client.Common;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Diff;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.Views.Diff.Dialogs;
using Unity.PlasticSCM.Editor.Tool;

namespace Unity.PlasticSCM.Editor.Views.Diff
{
    internal class DiffPanel :
        IDiffTreeViewMenuOperations,
        DiffTreeViewMenu.IMetaMenuOperations,
        UndeleteClientDiffsOperation.IGetRestorePathDialog
    {
        internal DiffPanel(
            WorkspaceInfo wkInfo,
            IWorkspaceWindow workspaceWindow,
            IViewSwitcher viewSwitcher,
            IHistoryViewLauncher historyViewLauncher,
            EditorWindow parentWindow,
            bool isGluonMode)
        {
            mWkInfo = wkInfo;
            mWorkspaceWindow = workspaceWindow;
            mViewSwitcher = viewSwitcher;
            mHistoryViewLauncher = historyViewLauncher;
            mParentWindow = parentWindow;
            mGuiMessage = new UnityPlasticGuiMessage(parentWindow);
            mIsGluonMode = isGluonMode;

            BuildComponents();

            mProgressControls = new ProgressControlsForViews();
        }

        internal void ClearInfo()
        {
            ClearData();

            mParentWindow.Repaint();
        }

        internal void UpdateInfo(
            MountPointWithPath mountWithPath,
            ChangesetInfo csetInfo)
        {
            FillData(mountWithPath, csetInfo);

            mParentWindow.Repaint();
        }

        internal void OnDisable()
        {
            mSearchField.downOrUpArrowKeyPressed -=
                SearchField_OnDownOrUpArrowKeyPressed;
        }

        internal void Update()
        {
            mProgressControls.UpdateProgress(mParentWindow);
        }

        internal void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            DoActionsToolbar(
                mDiffs,
                mProgressControls,
                GetHeaderLabelText(mSelectedChangesetInfo),
                mIsSkipMergeTrackingButtonVisible,
                mIsSkipMergeTrackingButtonChecked,
                mSearchField,
                mDiffTreeView);

            DoDiffTreeViewArea(
                mDiffTreeView,
                mProgressControls.IsOperationRunning());

            if (mProgressControls.HasNotification())
            {
                DrawProgressForViews.ForNotificationArea(
                    mProgressControls.ProgressData);
            }

            EditorGUILayout.EndVertical();
        }

        SelectedDiffsGroupInfo IDiffTreeViewMenuOperations.GetSelectedDiffsGroupInfo()
        {
            return SelectedDiffsGroupInfo.BuildFromSelectedNodes(
                DiffSelection.GetSelectedDiffsWithoutMeta(mDiffTreeView));
        }

        void IDiffTreeViewMenuOperations.Diff()
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(mIsGluonMode))
                return;

            ClientDiffInfo clientDiffInfo =
                DiffSelection.GetSelectedDiff(mDiffTreeView);

            DiffOperation.DiffClientDiff(
                mWkInfo,
                clientDiffInfo.DiffWithMount.Mount.Mount,
                clientDiffInfo.DiffWithMount.Difference,
                xDiffLauncher: null,
                imageDiffLauncher: null);
        }

        void IDiffTreeViewMenuOperations.History()
        {
            ClientDiffInfo clientDiffInfo =
                DiffSelection.GetSelectedDiff(mDiffTreeView);

            mHistoryViewLauncher.ShowHistoryView(
                clientDiffInfo.DiffWithMount.Mount.RepSpec,
                clientDiffInfo.DiffWithMount.Difference.RevInfo.ItemId,
                clientDiffInfo.DiffWithMount.Difference.Path,
                clientDiffInfo.DiffWithMount.Difference.IsDirectory);
        }

        void IDiffTreeViewMenuOperations.RevertChanges()
        {
            RevertClientDiffsOperation.RevertChanges(
                mWkInfo,
                DiffSelection.GetSelectedDiffs(mDiffTreeView),
                mWorkspaceWindow,
                mProgressControls,
                mGuiMessage,
                AfterRevertOrUndeleteOperation);
        }

        void IDiffTreeViewMenuOperations.Undelete()
        {
            UndeleteClientDiffsOperation.Undelete(
                mWkInfo,
                DiffSelection.GetSelectedDiffs(mDiffTreeView),
                mWorkspaceWindow,
                mProgressControls,
                this,
                mGuiMessage,
                AfterRevertOrUndeleteOperation);
        }

        void IDiffTreeViewMenuOperations.UndeleteToSpecifiedPaths()
        {
            UndeleteClientDiffsOperation.UndeleteToSpecifiedPaths(
                mWkInfo,
                DiffSelection.GetSelectedDiffs(mDiffTreeView),
                mWorkspaceWindow,
                mProgressControls,
                this,
                mGuiMessage,
                AfterRevertOrUndeleteOperation);
        }

        bool DiffTreeViewMenu.IMetaMenuOperations.SelectionHasMeta()
        {
            return mDiffTreeView.SelectionHasMeta();
        }

        void DiffTreeViewMenu.IMetaMenuOperations.DiffMeta()
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(mIsGluonMode))
                return;

            ClientDiffInfo clientDiffInfo =
                DiffSelection.GetSelectedDiff(mDiffTreeView);

            ClientDiffInfo clientDiffInfoMeta =
                mDiffTreeView.GetMetaDiff(clientDiffInfo);

            DiffOperation.DiffClientDiff(
                mWkInfo,
                clientDiffInfoMeta.DiffWithMount.Mount.Mount,
                clientDiffInfoMeta.DiffWithMount.Difference,
                xDiffLauncher: null,
                imageDiffLauncher: null);
        }

        GetRestorePathData
            UndeleteClientDiffsOperation.IGetRestorePathDialog.GetRestorePath(
                string wkPath, string restorePath, string explanation,
                bool isDirectory, bool showSkipButton)
        {
            return GetRestorePathDialog.GetRestorePath(
                wkPath, restorePath, explanation, isDirectory,
                showSkipButton, mParentWindow);
        }

        void DiffTreeViewMenu.IMetaMenuOperations.HistoryMeta()
        {
            ClientDiffInfo clientDiffInfo =
                DiffSelection.GetSelectedDiff(mDiffTreeView);

            ClientDiffInfo clientDiffInfoMeta =
                mDiffTreeView.GetMetaDiff(clientDiffInfo);

            mHistoryViewLauncher.ShowHistoryView(
                clientDiffInfoMeta.DiffWithMount.Mount.RepSpec,
                clientDiffInfoMeta.DiffWithMount.Difference.RevInfo.ItemId,
                clientDiffInfoMeta.DiffWithMount.Difference.Path,
                clientDiffInfoMeta.DiffWithMount.Difference.IsDirectory);
        }

        void SearchField_OnDownOrUpArrowKeyPressed()
        {
            mDiffTreeView.SetFocusAndEnsureSelectedItem();
        }

        void AfterRevertOrUndeleteOperation()
        {
            RefreshAsset.UnityAssetDatabase();

            mViewSwitcher.ShowPendingChanges();
        }

        void ClearData()
        {
            mSelectedMountWithPath = null;
            mSelectedChangesetInfo = null;

            mDiffs = null;

            ClearDiffs();
        }

        void FillData(
            MountPointWithPath mountWithPath,
            ChangesetInfo csetInfo)
        {
            mSelectedMountWithPath = mountWithPath;
            mSelectedChangesetInfo = csetInfo;

            ((IProgressControls)mProgressControls).ShowProgress(
                PlasticLocalization.GetString(PlasticLocalization.Name.Loading));

            mIsSkipMergeTrackingButtonVisible = false;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(100);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    mDiffs = Plastic.API.GetChangesetDifferences(
                        mountWithPath, csetInfo);
                },
                /*afterOperationDelegate*/ delegate
                {
                    ((IProgressControls)mProgressControls).HideProgress();

                    if (waiter.Exception != null)
                    {
                        ExceptionsHandler.DisplayException(waiter.Exception);
                        return;
                    }

                    if (mSelectedMountWithPath != mountWithPath ||
                        mSelectedChangesetInfo != csetInfo)
                        return;

                    if (mDiffs == null || mDiffs.Count == 0)
                    {
                        ClearDiffs();
                        return;
                    }

                    mIsSkipMergeTrackingButtonVisible =
                        ClientDiffList.HasMerges(mDiffs);

                    bool skipMergeTracking =
                        mIsSkipMergeTrackingButtonVisible &&
                        mIsSkipMergeTrackingButtonChecked;

                    UpdateDiffTreeView(
                        mDiffs, skipMergeTracking, mDiffTreeView);
                });
        }

        void ClearDiffs()
        {
            mIsSkipMergeTrackingButtonVisible = false;

            ClearDiffTreeView(mDiffTreeView);

            ((IProgressControls)mProgressControls).ShowNotification(
                PlasticLocalization.GetString(PlasticLocalization.Name.NoContentToCompare));
        }

        static void ClearDiffTreeView(
            DiffTreeView diffTreeView)
        {
            diffTreeView.ClearModel();

            diffTreeView.Reload();
        }

        static void UpdateDiffTreeView(
            List<ClientDiff> diffs,
            bool skipMergeTracking,
            DiffTreeView diffTreeView)
        {
            diffTreeView.BuildModel(
                diffs, skipMergeTracking);

            diffTreeView.Refilter();

            diffTreeView.Sort();

            diffTreeView.Reload();
        }

        static string GetHeaderLabelText(
            ChangesetInfo changesetInfo)
        {
            if (changesetInfo == null)
                return PlasticLocalization.GetString(PlasticLocalization.Name.ChangesLabelPlural);

            return string.Format(
                PlasticLocalization.GetString(PlasticLocalization.Name.ChangesOfChangeset),
                changesetInfo.ChangesetId);
        }

        void DoActionsToolbar(
            List<ClientDiff> diffs,
            ProgressControlsForViews progressControls,
            string headerLabelText,
            bool isSkipMergeTrackingButtonVisible,
            bool isSkipMergeTrackingButtonChecked,
            SearchField searchField,
            DiffTreeView diffTreeView)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label(headerLabelText,
                UnityStyles.DiffPanel.HeaderLabel);

            if (progressControls.IsOperationRunning())
            {
                DrawProgressForViews.ForIndeterminateProgress(
                    progressControls.ProgressData);
            }

            GUILayout.FlexibleSpace();

            if (isSkipMergeTrackingButtonVisible)
            {
                DoSkipMergeTrackingButton(
                    diffs,
                    isSkipMergeTrackingButtonChecked,
                    diffTreeView);
            }

            DrawSearchField.For(
                searchField,
                diffTreeView,
                UnityConstants.SEARCH_FIELD_WIDTH);

            EditorGUILayout.EndHorizontal();
        }

        void DoSkipMergeTrackingButton(
            List<ClientDiff> diffs,
            bool isSkipMergeTrackingButtonChecked,
            DiffTreeView diffTreeView)
        {
            bool wasChecked = isSkipMergeTrackingButtonChecked;

            GUIContent buttonContent = new GUIContent(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.SkipDiffMergeTracking));

            GUIStyle buttonStyle = EditorStyles.toolbarButton;

            float buttonWidth = buttonStyle.CalcSize(buttonContent).x + 10;

            Rect toggleRect = GUILayoutUtility.GetRect(
                buttonContent, buttonStyle, GUILayout.Width(buttonWidth));

            bool isChecked = GUI.Toggle(
                toggleRect, wasChecked, buttonContent, buttonStyle);

            if (wasChecked == isChecked)
                return;

            UpdateDiffTreeView(diffs, isChecked, diffTreeView);

            mIsSkipMergeTrackingButtonChecked = isChecked;
        }

        static void DoDiffTreeViewArea(
            DiffTreeView diffTreeView,
            bool isOperationRunning)
        {
            GUI.enabled = !isOperationRunning;

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            diffTreeView.OnGUI(rect);

            GUI.enabled = true;
        }

        void BuildComponents()
        {
            mSearchField = new SearchField();
            mSearchField.downOrUpArrowKeyPressed += SearchField_OnDownOrUpArrowKeyPressed;

            mDiffTreeView = new DiffTreeView(
                new DiffTreeViewMenu(this, this, mIsGluonMode));
            mDiffTreeView.Reload();
        }

        volatile List<ClientDiff> mDiffs;

        bool mIsSkipMergeTrackingButtonVisible;
        bool mIsSkipMergeTrackingButtonChecked;

        SearchField mSearchField;
        DiffTreeView mDiffTreeView;

        ChangesetInfo mSelectedChangesetInfo;
        MountPointWithPath mSelectedMountWithPath;

        readonly ProgressControlsForViews mProgressControls;
        readonly GuiMessage.IGuiMessage mGuiMessage;
        readonly EditorWindow mParentWindow;
        readonly IWorkspaceWindow mWorkspaceWindow;
        readonly IHistoryViewLauncher mHistoryViewLauncher;
        readonly IViewSwitcher mViewSwitcher;
        readonly WorkspaceInfo mWkInfo;
        readonly bool mIsGluonMode;
    }
}