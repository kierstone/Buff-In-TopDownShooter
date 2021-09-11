using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Codice.Client.Common.FsNodeReaders;
using Codice.CM.Common;
using GluonGui;
using PlasticGui;
using PlasticGui.Gluon.WorkspaceWindow;
using PlasticGui.Gluon.WorkspaceWindow.Views.IncomingChanges;
using PlasticGui.WorkspaceWindow.Diff;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.Views.IncomingChanges.Gluon.Errors;
using Unity.PlasticSCM.Editor.Tool;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Gluon
{
    internal class IncomingChangesTab :
        IIncomingChangesTab,
        IRefreshableView,
        IncomingChangesViewLogic.IIncomingChangesView,
        IIncomingChangesViewMenuOperations,
        IncomingChangesViewMenu.IMetaMenuOperations
    {
        internal IncomingChangesTab(
            WorkspaceInfo wkInfo,
            ViewHost viewHost,
            WorkspaceWindow workspaceWindow,
            NewIncomingChangesUpdater newIncomingChangesUpdater,
            CheckIncomingChanges.IUpdateIncomingChanges updateIncomingChanges,
            EditorWindow parentWindow)
        {
            mWkInfo = wkInfo;
            mNewIncomingChangesUpdater = newIncomingChangesUpdater;
            mParentWindow = parentWindow;

            BuildComponents();

            mProgressControls = new ProgressControlsForViews();

            mErrorsSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.75f, 0.25f },
                new int[] { 100, 100 },
                new int[] { 100000, 100000 }
            );

            mErrorDetailsSplitterState = PlasticSplitterGUILayout.InitSplitterState(
                new float[] { 0.60f, 0.40f },
                new int[] { 100, 100 },
                new int[] { 100000, 100000 }
            );

            mIncomingChangesViewLogic = new IncomingChangesViewLogic(
                wkInfo, viewHost, this, new UnityPlasticGuiMessage(parentWindow),
                mProgressControls, updateIncomingChanges,
                workspaceWindow.GluonProgressOperationHandler, workspaceWindow,
                new IncomingChangesViewLogic.ApplyWorkspaceLocalChanges(),
                new IncomingChangesViewLogic.OutOfDateItemsOperations(),
                new IncomingChangesViewLogic.ResolveUserName(),
                new IncomingChangesViewLogic.GetWorkingBranch(),
                NewChangesInWk.Build(wkInfo, new BuildWorkspacekIsRelevantNewChange()),
                null);

            mIncomingChangesViewLogic.Refresh();
        }

        bool IIncomingChangesTab.IsVisible
        {
            get { return mIsVisible; }
            set { mIsVisible = value; }
        }

        void IIncomingChangesTab.OnDisable()
        {
            TreeHeaderSettings.Save(
                mIncomingChangesTreeView.multiColumnHeader.state,
                UnityConstants.GLUON_INCOMING_CHANGES_TABLE_SETTINGS_NAME);

            TreeHeaderSettings.Save(
                mErrorsListView.multiColumnHeader.state,
                UnityConstants.GLUON_INCOMING_ERRORS_TABLE_SETTINGS_NAME);
        }

        void IIncomingChangesTab.Update()
        {
            mProgressControls.UpdateProgress(mParentWindow);
        }

        void IIncomingChangesTab.OnGUI()
        {
            DoActionsToolbar(
                mIsProcessMergesButtonVisible,
                mIsCancelMergesButtonVisible,
                mIsProcessMergesButtonEnabled,
                mIsCancelMergesButtonEnabled,
                mProcessMergesButtonText,
                mIncomingChangesViewLogic,
                mIncomingChangesTreeView,
                mProgressControls);

            bool splitterNeeded = mIsErrorsListVisible;

            if (splitterNeeded)
                PlasticSplitterGUILayout.BeginVerticalSplit(mErrorsSplitterState);

            DoIncomingChangesArea(
                mIncomingChangesTreeView,
                mPendingConflictsLabelData,
                mChangesToApplySummaryLabelText,
                mMessageLabelText,
                mIsMessageLabelVisible,
                mProgressControls.IsOperationRunning());

            DoErrorsArea(
                mErrorsListView,
                mErrorDetailsSplitterState,
                mErrorMessageLabelText,
                mIsErrorsListVisible,
                mIsErrorMessageLabelVisible);

            if (splitterNeeded)
                PlasticSplitterGUILayout.EndVerticalSplit();

            if (mProgressControls.HasNotification())
            {
                DrawProgressForViews.ForNotificationArea(
                    mProgressControls.ProgressData);
            }
        }

        void IIncomingChangesTab.AutoRefresh()
        {
            mIncomingChangesViewLogic.AutoRefresh(DateTime.Now);
        }

        void IRefreshableView.Refresh()
        {
            if (mNewIncomingChangesUpdater != null)
                mNewIncomingChangesUpdater.Update(DateTime.Now);

            mIncomingChangesViewLogic.Refresh();
        }

        void IncomingChangesViewLogic.IIncomingChangesView.UpdateData(
            IncomingChangesTree tree,
            List<ErrorMessage> errorMessages,
            string processMergesButtonText,
            PendingConflictsLabelData conflictsLabelData,
            string changesToApplySummaryText)
        {
            ShowProcessMergesButton(processMergesButtonText);

            ((IncomingChangesViewLogic.IIncomingChangesView)this).
                UpdatePendingConflictsLabel(conflictsLabelData);

            mChangesToApplySummaryLabelText = changesToApplySummaryText;

            UpdateIncomingChangesTree(mIncomingChangesTreeView, tree);

            UpdateErrorsList(mErrorsListView, errorMessages);

            mIsErrorsListVisible = errorMessages.Count > 0;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.UpdatePendingConflictsLabel(
            PendingConflictsLabelData data)
        {
            mPendingConflictsLabelData = data;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.UpdateSolvedFileConflicts(
            List<IncomingChangeInfo> solvedConflicts,
            IncomingChangeInfo currentConflict)
        {
            mIncomingChangesTreeView.UpdateSolvedFileConflicts(
                solvedConflicts, currentConflict);
        }

        void IncomingChangesViewLogic.IIncomingChangesView.ShowMessage(
            string message, bool isErrorMessage)
        {
            if (isErrorMessage)
            {
                mErrorMessageLabelText = message;
                mIsErrorMessageLabelVisible = true;
                return;
            }

            mMessageLabelText = message;
            mIsMessageLabelVisible = true;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.HideMessage()
        {
            mMessageLabelText = string.Empty;
            mIsMessageLabelVisible = false;

            mErrorMessageLabelText = string.Empty;
            mIsErrorMessageLabelVisible = false;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.DisableProcessMergesButton()
        {
            mIsProcessMergesButtonEnabled = false;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.ShowCancelButton()
        {
            mIsCancelMergesButtonEnabled = true;
            mIsCancelMergesButtonVisible = true;
        }

        void IncomingChangesViewLogic.IIncomingChangesView.HideCancelButton()
        {
            mIsCancelMergesButtonEnabled = false;
            mIsCancelMergesButtonVisible = false;
        }

        SelectedIncomingChangesGroupInfo IIncomingChangesViewMenuOperations.GetSelectedIncomingChangesGroupInfo()
        {
            return IncomingChangesSelection.GetSelectedGroupInfo(mIncomingChangesTreeView);
        }

        void IIncomingChangesViewMenuOperations.MergeContributors()
        {
            List<IncomingChangeInfo> fileConflicts = IncomingChangesSelection.
                GetSelectedFileConflictsIncludingMeta(mIncomingChangesTreeView);

            mIncomingChangesViewLogic.ProcessMergesForConflicts(
                MergeContributorType.MergeContributors,
                fileConflicts,
                RefreshAsset.UnityAssetDatabase);
        }

        void IIncomingChangesViewMenuOperations.MergeKeepingSourceChanges()
        {
            List<IncomingChangeInfo> fileConflicts = IncomingChangesSelection.
                GetSelectedFileConflictsIncludingMeta(mIncomingChangesTreeView);

            mIncomingChangesViewLogic.ProcessMergesForConflicts(
                MergeContributorType.KeepSource,
                fileConflicts,
                RefreshAsset.UnityAssetDatabase);
        }

        void IIncomingChangesViewMenuOperations.MergeKeepingWorkspaceChanges()
        {
            List<IncomingChangeInfo> fileConflicts = IncomingChangesSelection.
                GetSelectedFileConflictsIncludingMeta(mIncomingChangesTreeView);

            mIncomingChangesViewLogic.ProcessMergesForConflicts(
                MergeContributorType.KeepDestination,
                fileConflicts,
                RefreshAsset.UnityAssetDatabase);
        }

        void IIncomingChangesViewMenuOperations.DiffIncomingChanges()
        {
            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffIncomingChanges(
                incomingChange,
                mWkInfo);
        }

        void IIncomingChangesViewMenuOperations.DiffYoursWithIncoming()
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(true))
                return;

            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffYoursWithIncoming(
                incomingChange,
                mWkInfo);
        }

        void IncomingChangesViewMenu.IMetaMenuOperations.DiffIncomingChanges()
        {
            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffIncomingChanges(
                mIncomingChangesTreeView.GetMetaChange(incomingChange),
                mWkInfo);
        }

        void IncomingChangesViewMenu.IMetaMenuOperations.DiffYoursWithIncoming()
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(true))
                return;

            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffYoursWithIncoming(
                mIncomingChangesTreeView.GetMetaChange(incomingChange),
                mWkInfo);
        }

        bool IncomingChangesViewMenu.IMetaMenuOperations.SelectionHasMeta()
        {
            return mIncomingChangesTreeView.SelectionHasMeta();
        }

        static void DiffIncomingChanges(
            IncomingChangeInfo incomingChange,
            WorkspaceInfo wkInfo)
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(true))
                return;

            DiffOperation.DiffRevisions(
                wkInfo,
                incomingChange.GetMount().RepSpec,
                incomingChange.GetBaseRevision(),
                incomingChange.GetRevision(),
                incomingChange.GetPath(),
                incomingChange.GetPath(),
                true,
                xDiffLauncher: null,
                imageDiffLauncher: null);
        }

        static void DiffYoursWithIncoming(
            IncomingChangeInfo incomingChange,
            WorkspaceInfo wkInfo)
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(true))
                return;

            DiffOperation.DiffYoursWithIncoming(
                wkInfo,
                incomingChange.GetMount(),
                incomingChange.GetRevision(),
                incomingChange.GetPath(),
                xDiffLauncher: null,
                imageDiffLauncher: null);
        }

        static void UpdateErrorsList(
            ErrorsListView errorsListView,
            List<ErrorMessage> errorMessages)
        {
            errorsListView.BuildModel(errorMessages);

            errorsListView.Reload();
        }

        void UpdateProcessMergesButtonText()
        {
            mProcessMergesButtonText =
                mIncomingChangesViewLogic.GetProcessMergesButtonText();
        }

        void ShowProcessMergesButton(string processMergesButtonText)
        {
            mProcessMergesButtonText = processMergesButtonText;
            mIsProcessMergesButtonEnabled = true;
            mIsProcessMergesButtonVisible = true;
        }

        void DoActionsToolbar(
            bool isProcessMergesButtonVisible,
            bool isCancelMergesButtonVisible,
            bool isProcessMergesButtonEnabled,
            bool isCancelMergesButtonEnabled,
            string processMergesButtonText,
            IncomingChangesViewLogic incomingChangesViewLogic,
            IncomingChangesTreeView incomingChangesTreeView,
            ProgressControlsForViews progressControls)
        {
            GUIStyle guiStyle = new GUIStyle();
            guiStyle.margin = new RectOffset(5, 5, 5, 5);

            EditorGUILayout.BeginHorizontal(guiStyle);

            if (isProcessMergesButtonVisible)
            {
                DoProcessMergesButton(
                    isProcessMergesButtonEnabled,
                    processMergesButtonText,
                    incomingChangesViewLogic,
                    incomingChangesTreeView);
            }

            if (isCancelMergesButtonVisible)
            {
                DoCancelMergesButton(
                    isCancelMergesButtonEnabled,
                    incomingChangesViewLogic);
            }

            if (progressControls.IsOperationRunning())
            {
                DrawProgressForViews.ForIndeterminateProgress(
                    progressControls.ProgressData);
            }

            GUILayout.FlexibleSpace();

            DoRefreshButton(
                !progressControls.IsOperationRunning(),
                incomingChangesViewLogic);

            EditorGUILayout.EndHorizontal();
        }

        static void DoIncomingChangesArea(
            IncomingChangesTreeView incomingChangesTreeView,
            PendingConflictsLabelData pendingConflictsLabelData,
            string changesToApplySummaryLabelText,
            string messageLabelText,
            bool isMessageLabelVisible,
            bool isOperationRunning)
        {
            EditorGUILayout.BeginVertical();

            DoPendingConflictsAndChangesToApplyLabel(
                pendingConflictsLabelData,
                changesToApplySummaryLabelText);

            DoIncomingChangesTreeViewArea(
                incomingChangesTreeView,
                isOperationRunning);

            if (isMessageLabelVisible)
                DoInfoMessageArea(messageLabelText);

            EditorGUILayout.EndVertical();
        }

        static void DoProcessMergesButton(
            bool isEnabled,
            string processMergesButtonText,
            IncomingChangesViewLogic incomingChangesViewLogic,
            IncomingChangesTreeView incomingChangesTreeView)
        {
            GUI.enabled = isEnabled;

            if (DrawActionButton.For(processMergesButtonText))
            {
                List<IncomingChangeInfo> incomingChanges =
                    incomingChangesViewLogic.GetCheckedChanges();

                incomingChangesTreeView.FillWithMeta(incomingChanges);

                if (incomingChanges.Count == 0)
                    return;

                incomingChangesViewLogic.ProcessMergesForItems(
                    incomingChanges,
                    RefreshAsset.UnityAssetDatabase);
            }

            GUI.enabled = true;
        }

        void DoCancelMergesButton(
            bool isEnabled,
            IncomingChangesViewLogic incomingChangesViewLogic)
        {
            GUI.enabled = isEnabled;

            if (DrawActionButton.For(PlasticLocalization.GetString(
                    PlasticLocalization.Name.CancelButton)))
            {
                incomingChangesViewLogic.Cancel();

                mIsCancelMergesButtonEnabled = false;
            }

            GUI.enabled = true;
        }

        void DoErrorsArea(
            ErrorsListView errorsListView,
            object splitterState,
            string errorMessageLabelText,
            bool isErrorsListVisible,
            bool isErrorMessageLabelVisible)
        {
            EditorGUILayout.BeginVertical();

            if (isErrorsListVisible)
            {
                DrawSplitter.ForHorizontalIndicator();
                DoErrorsListArea(errorsListView, splitterState);
            }

            if (isErrorMessageLabelVisible)
                DoErrorMessageArea(errorMessageLabelText);

            EditorGUILayout.EndVertical();
        }

        static void DoPendingConflictsAndChangesToApplyLabel(
            PendingConflictsLabelData pendingConflictsLabelData,
            string changesToApplySummaryLabelText)
        {
            EditorGUILayout.BeginHorizontal();

            GUIStyle pendingConflictsOfTotalStyle =
                pendingConflictsLabelData.HasPendingConflicts ?
                UnityStyles.IncomingChangesTab.RedPendingConflictsOfTotalLabel :
                UnityStyles.IncomingChangesTab.GreenPendingConflictsOfTotalLabel;

            GUILayout.Label(
                pendingConflictsLabelData.PendingConflictsOfTotalText,
                pendingConflictsOfTotalStyle);

            GUILayout.Label(
                pendingConflictsLabelData.PendingConflictsLabelText,
                UnityStyles.IncomingChangesTab.PendingConflictsLabel);

            GUILayout.Space(5);

            GUILayout.Label(
                changesToApplySummaryLabelText,
                UnityStyles.IncomingChangesTab.ChangesToApplySummaryLabel);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        static void UpdateIncomingChangesTree(
            IncomingChangesTreeView incomingChangesTreeView,
            IncomingChangesTree tree)
        {
            incomingChangesTreeView.BuildModel(
                UnityIncomingChangesTree.BuildIncomingChangeCategories(tree));
            incomingChangesTreeView.Sort();
            incomingChangesTreeView.Reload();
        }

        static void DoIncomingChangesTreeViewArea(
            IncomingChangesTreeView incomingChangesTreeView,
            bool isOperationRunning)
        {
            GUI.enabled = !isOperationRunning;

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            incomingChangesTreeView.OnGUI(rect);

            GUI.enabled = true;
        }

        void DoErrorsListArea(
            ErrorsListView errorsListView,
            object splitterState)
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Label(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.IncomingChangesCannotBeApplied),
                EditorStyles.boldLabel);

            DoErrorsListSplitViewArea(
                errorsListView, splitterState);

            EditorGUILayout.EndVertical();
        }

        static void DoErrorMessageArea(string message)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.HelpBox(message, MessageType.Error);

            EditorGUILayout.EndHorizontal();
        }

        void DoErrorsListSplitViewArea(
            ErrorsListView errorsListView,
            object splitterState)
        {
            EditorGUILayout.BeginHorizontal();

            PlasticSplitterGUILayout.BeginHorizontalSplit(splitterState);

            DoErrorsListViewArea(errorsListView);

            DoErrorDetailsTextArea(errorsListView.GetSelectedError());

            PlasticSplitterGUILayout.EndHorizontalSplit();

            EditorGUILayout.EndHorizontal();
        }

        static void DoErrorsListViewArea(
            ErrorsListView errorsListView)
        {
            Rect treeRect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            errorsListView.OnGUI(treeRect);
        }

        void DoErrorDetailsTextArea(ErrorMessage selectedErrorMessage)
        {
            string errorDetailsText = selectedErrorMessage == null ?
                string.Empty : selectedErrorMessage.Error;

            mErrorDetailsScrollPosition = GUILayout.BeginScrollView(
                mErrorDetailsScrollPosition);

            GUILayout.TextArea(
                errorDetailsText, UnityStyles.TextFieldWithWrapping,
                GUILayout.ExpandHeight(true));

            GUILayout.EndScrollView();
        }

        static void DoRefreshButton(
            bool isEnabled,
            IncomingChangesViewLogic incomingChangesViewLogic)
        {
            GUI.enabled = isEnabled;

            if (GUILayout.Button(new GUIContent(
                    Images.GetRefreshIcon())))
                incomingChangesViewLogic.Refresh();

            GUI.enabled = true;
        }

        static void DoInfoMessageArea(string message)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.HelpBox(message, MessageType.Info);

            EditorGUILayout.EndHorizontal();
        }

        void BuildComponents()
        {
            IncomingChangesTreeHeaderState incomingChangesHeaderState =
                IncomingChangesTreeHeaderState.GetDefault();
            TreeHeaderSettings.Load(incomingChangesHeaderState,
                UnityConstants.GLUON_INCOMING_CHANGES_TABLE_SETTINGS_NAME,
                (int)IncomingChangesTreeColumn.Path, true);

            mIncomingChangesTreeView = new IncomingChangesTreeView(
                mWkInfo, incomingChangesHeaderState,
                IncomingChangesTreeHeaderState.GetColumnNames(),
                new IncomingChangesViewMenu(this, this),
                UpdateProcessMergesButtonText);
            mIncomingChangesTreeView.Reload();

            ErrorsListHeaderState errorsListHeaderState =
                ErrorsListHeaderState.GetDefault();
            TreeHeaderSettings.Load(errorsListHeaderState,
                UnityConstants.GLUON_INCOMING_ERRORS_TABLE_SETTINGS_NAME,
                UnityConstants.UNSORT_COLUMN_ID);

            mErrorsListView = new ErrorsListView(errorsListHeaderState);
            mErrorsListView.Reload();
        }

        bool mIsVisible;
        bool mIsProcessMergesButtonVisible;
        bool mIsCancelMergesButtonVisible;
        bool mIsMessageLabelVisible;
        bool mIsErrorMessageLabelVisible;
        bool mIsErrorsListVisible;

        bool mIsProcessMergesButtonEnabled;
        bool mIsCancelMergesButtonEnabled;

        string mProcessMergesButtonText;
        string mMessageLabelText;
        string mErrorMessageLabelText;

        PendingConflictsLabelData mPendingConflictsLabelData;
        string mChangesToApplySummaryLabelText;

        IncomingChangesTreeView mIncomingChangesTreeView;
        ErrorsListView mErrorsListView;

        object mErrorsSplitterState;
        object mErrorDetailsSplitterState;

        readonly ProgressControlsForViews mProgressControls;
        Vector2 mErrorDetailsScrollPosition;

        readonly IncomingChangesViewLogic mIncomingChangesViewLogic;
        readonly EditorWindow mParentWindow;
        readonly NewIncomingChangesUpdater mNewIncomingChangesUpdater;
        readonly WorkspaceInfo mWkInfo;
    }
}