using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Codice.Client.BaseCommands;
using Codice.Client.BaseCommands.Merge;
using Codice.Client.Commands;
using Codice.Client.Common;
using Codice.Client.Common.FsNodeReaders;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using Codice.CM.Common.Merge;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.BranchExplorer;
using PlasticGui.WorkspaceWindow.Diff;
using PlasticGui.WorkspaceWindow.IncomingChanges;
using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer.DirectoryConflicts;
using Unity.PlasticSCM.Editor.Tool;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer
{
    internal class IncomingChangesTab :
        IIncomingChangesTab,
        IRefreshableView,
        MergeViewLogic.IMergeView,
        IncomingChangesTree.IGetConflictResolution,
        IIncomingChangesViewMenuOperations,
        IncomingChangesViewMenu.IMetaMenuOperations
    {
        internal IncomingChangesTab(
            WorkspaceInfo wkInfo,
            IWorkspaceWindow workspaceWindow,
            IViewSwitcher switcher,
            NewIncomingChangesUpdater newIncomingChangesUpdater,
            EditorWindow parentWindow)
        {
            mWkInfo = wkInfo;
            mWorkspaceWindow = workspaceWindow;
            mSwitcher = switcher;
            mNewIncomingChangesUpdater = newIncomingChangesUpdater;
            mParentWindow = parentWindow;
            mGuiMessage = new UnityPlasticGuiMessage(parentWindow);

            BuildComponents(mWkInfo);

            mProgressControls = new ProgressControlsForViews();

            PlasticNotifier plasticNotifier = new PlasticNotifier();

            mMergeController = new MergeController(
                mWkInfo,
                null,
                null,
                EnumMergeType.IncomingMerge,
                true,
                plasticNotifier,
                null);

            mMergeViewLogic = new MergeViewLogic(
                mWkInfo,
                EnumMergeType.IncomingMerge,
                true,
                mMergeController,
                plasticNotifier,
                ShowIncomingChangesFrom.NotificationBar,
                null,
                mNewIncomingChangesUpdater,
                null,
                this,
                NewChangesInWk.Build(mWkInfo, new BuildWorkspacekIsRelevantNewChange()),
                mProgressControls,
                null);

            ((IRefreshableView)this).Refresh();
        }

        bool IIncomingChangesTab.IsVisible{ get; set; }

        void IIncomingChangesTab.OnDisable()
        {
            TreeHeaderSettings.Save(
                mIncomingChangesTreeView.multiColumnHeader.state,
                UnityConstants.DEVELOPER_INCOMING_CHANGES_TABLE_SETTINGS_NAME);
        }

        void IIncomingChangesTab.Update()
        {
            mProgressControls.UpdateProgress(mParentWindow);
        }

        void IIncomingChangesTab.OnGUI()
        {
            if (Event.current.type == EventType.Layout)
            {
                mHasPendingDirectoryConflicts =
                    MergeTreeResultParser.GetUnsolvedDirectoryConflictsCount(mResultConflicts) > 0;
                mIsOperationRunning = mProgressControls.IsOperationRunning();
            }

            DoActionsToolbar(
                mIsProcessMergesButtonVisible,
                mIsCancelMergesButtonVisible,
                mIsProcessMergesButtonEnabled,
                mIsCancelMergesButtonEnabled,
                mProcessMergesButtonText,
                mHasPendingDirectoryConflicts,
                mIsOperationRunning,
                mWorkspaceWindow,
                mMergeViewLogic,
                mProgressControls.ProgressData);

            DoFileConflictsArea(
                mIncomingChangesTreeView,
                mResultConflicts,
                mSolvedFileConflicts,
                mRootMountPoint,
                mIsOperationRunning);

            List<IncomingChangeInfo> selectedIncomingChanges =
                mIncomingChangesTreeView.GetSelectedIncomingChanges();

            if (GetSelectedIncomingChangesGroupInfo.For(
                    selectedIncomingChanges).IsDirectoryConflictsSelection &&
                !Mouse.IsRightMouseButtonPressed(Event.current))
            {
                DoDirectoryConflictResolutionPanel(
                    selectedIncomingChanges,
                    new Action<IncomingChangeInfo>(ResolveDirectoryConflict),
                    mConflictResolutionStates);
            }

            if (mIsMessageLabelVisible)
                DoInfoMessageArea(mMessageLabelText);

            if (mIsErrorMessageLabelVisible)
                DoErrorMessageArea(mErrorMessageLabelText);

            if (mProgressControls.HasNotification())
            {
                DrawProgressForViews.ForNotificationArea(
                    mProgressControls.ProgressData);
            }
        }

        void IIncomingChangesTab.AutoRefresh()
        {
            BranchInfo workingBranch = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    workingBranch = OverlappedCalculator.GetWorkingBranch(
                        mWkInfo.ClientPath);
                },
                /*afterOperationDelegate*/ delegate
                {
                    if (waiter.Exception != null)
                    {
                        ExceptionsHandler.DisplayException(waiter.Exception);
                        return;
                    }

                    mMergeController.UpdateMergeObjectInfoIfNeeded(workingBranch);
                    mMergeViewLogic.AutoRefresh();
                });
        }

        void IRefreshableView.Refresh()
        {
            BranchInfo workingBranch = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    workingBranch = OverlappedCalculator.GetWorkingBranch(
                        mWkInfo.ClientPath);
                },
                /*afterOperationDelegate*/ delegate
                {
                    if (waiter.Exception != null)
                    {
                        ExceptionsHandler.DisplayException(waiter.Exception);
                        return;
                    }

                    mMergeController.UpdateMergeObjectInfoIfNeeded(workingBranch);
                    mMergeViewLogic.Refresh();
                });
        }

        void MergeViewLogic.IMergeView.UpdateData(
            MergeTreeResult resultConflicts,
            ExplainMergeData explainMergeData,
            MergeSolvedFileConflicts solvedFileConflicts,
            MountPointWithPath rootMountPoint,
            bool isIncomingMerge,
            bool isMergeTo,
            bool isUpdateMerge,
            bool mergeHasFinished)
        {
            HideMessage();

            ShowProcessMergesButton(MergeViewTexts.GetProcessMergeButtonText(
                MergeTreeResultParser.GetFileConflictsCount(resultConflicts) > 0,
                true));

            mResultConflicts = resultConflicts;
            mSolvedFileConflicts = solvedFileConflicts;
            mRootMountPoint = rootMountPoint;
            mConflictResolutionStates.Clear();

            UpdateFileConflictsTree(
                IncomingChangesTree.BuildIncomingChangeCategories(
                    mResultConflicts,
                    this,
                    mRootMountPoint),
                mIncomingChangesTreeView);
        }

        void MergeViewLogic.IMergeView.UpdateSolvedFileConflicts(
            MergeSolvedFileConflicts solvedFileConflicts)
        {
            mIncomingChangesTreeView.UpdateSolvedFileConflicts(
                solvedFileConflicts);
        }

        void MergeViewLogic.IMergeView.ShowMessage(
            string title,
            string message,
            bool isErrorMessage)
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

        string MergeViewLogic.IMergeView.GetComments(out bool bCancel)
        {
            bCancel = false;
            return string.Empty;
        }

        void MergeViewLogic.IMergeView.DisableProcessMergesButton()
        {
            mIsProcessMergesButtonEnabled = false;
        }

        void MergeViewLogic.IMergeView.ShowCancelButton()
        {
            mIsCancelMergesButtonEnabled = true;
            mIsCancelMergesButtonVisible = true;
        }

        void MergeViewLogic.IMergeView.HideCancelButton()
        {
            mIsCancelMergesButtonEnabled = false;
            mIsCancelMergesButtonVisible = false;
        }

        SelectedIncomingChangesGroupInfo IIncomingChangesViewMenuOperations.GetSelectedIncomingChangesGroupInfo()
        {
            return IncomingChangesSelection.GetSelectedGroupInfo(mIncomingChangesTreeView);
        }

        string IncomingChangesTree.IGetConflictResolution.GetConflictResolution(
            DirectoryConflict conflict)
        {
            return mMergeViewLogic.GetConflictResolution(conflict);
        }

        void IIncomingChangesViewMenuOperations.MergeContributors()
        {
            List<string> selectedPaths = IncomingChangesSelection.
                GetPathsFromSelectedFileConflictsIncludingMeta(
                    mIncomingChangesTreeView);

            mMergeViewLogic.ProcessMerges(
                mWorkspaceWindow,
                mSwitcher,
                mGuiMessage,
                selectedPaths,
                MergeContributorType.MergeContributors,
                RefreshAsset.UnityAssetDatabase);
        }

        void IIncomingChangesViewMenuOperations.MergeKeepingSourceChanges()
        {
            List<string> selectedPaths = IncomingChangesSelection.
                GetPathsFromSelectedFileConflictsIncludingMeta(
                    mIncomingChangesTreeView);

            mMergeViewLogic.ProcessMerges(
                mWorkspaceWindow,
                mSwitcher,
                mGuiMessage,
                selectedPaths,
                MergeContributorType.KeepSource,
                RefreshAsset.UnityAssetDatabase);
        }

        void IIncomingChangesViewMenuOperations.MergeKeepingWorkspaceChanges()
        {
            List<string> selectedPaths = IncomingChangesSelection.
                GetPathsFromSelectedFileConflictsIncludingMeta(
                    mIncomingChangesTreeView);

            mMergeViewLogic.ProcessMerges(
                mWorkspaceWindow,
                mSwitcher,
                mGuiMessage,
                selectedPaths,
                MergeContributorType.KeepDestination,
                RefreshAsset.UnityAssetDatabase);
        }

        void IIncomingChangesViewMenuOperations.DiffYoursWithIncoming()
        {
            IncomingChangeInfo incomingChange = IncomingChangesSelection.
                GetSingleSelectedIncomingChange(mIncomingChangesTreeView);

            if (incomingChange == null)
                return;

            DiffYoursWithIncoming(
                incomingChange,
                mWkInfo);
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

        static void DiffYoursWithIncoming(
            IncomingChangeInfo incomingChange,
            WorkspaceInfo wkInfo)
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(false))
                return;

            DiffOperation.DiffYoursWithIncoming(
                wkInfo,
                incomingChange.GetMount(),
                incomingChange.GetRevision(),
                incomingChange.GetPath(),
                xDiffLauncher: null,
                imageDiffLauncher: null);
        }

        static void DiffIncomingChanges(
            IncomingChangeInfo incomingChange,
            WorkspaceInfo wkInfo)
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(false))
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

        void ShowProcessMergesButton(string processMergesButtonText)
        {
            mProcessMergesButtonText = processMergesButtonText;
            mIsProcessMergesButtonEnabled = true;
            mIsProcessMergesButtonVisible = true;
        }

        void HideMessage()
        {
            mMessageLabelText = string.Empty;
            mIsMessageLabelVisible = false;

            mErrorMessageLabelText = string.Empty;
            mIsErrorMessageLabelVisible = false;
        }

        static void DoDirectoryConflictResolutionPanel(
            List<IncomingChangeInfo> selectedChangeInfos,
            Action<IncomingChangeInfo> resolveDirectoryConflictAction,
            Dictionary<DirectoryConflict, ConflictResolutionState> conflictResolutionStates)
        {
            IncomingChangeInfo selectedDirectoryConflict = selectedChangeInfos[0];

            if (selectedDirectoryConflict.DirectoryConflict.IsResolved())
                return;

            DirectoryConflictUserInfo conflictUserInfo;
            DirectoryConflictAction[] conflictActions;

            DirectoryConflictResolutionInfo.FromDirectoryConflict(
                selectedDirectoryConflict.GetMount(),
                selectedDirectoryConflict.DirectoryConflict,
                out conflictUserInfo,
                out conflictActions);

            ConflictResolutionState conflictResolutionState = GetConflictResolutionState(
                selectedDirectoryConflict.DirectoryConflict,
                conflictActions,
                conflictResolutionStates);

            int pendingSelectedConflictsCount = GetPendingConflictsCount(
                selectedChangeInfos);

            DrawDirectoryResolutionPanel.ForConflict(
                selectedDirectoryConflict,
                (pendingSelectedConflictsCount <= 1) ? 0 : pendingSelectedConflictsCount - 1,
                conflictUserInfo,
                conflictActions,
                resolveDirectoryConflictAction,
                ref conflictResolutionState);
        }

        void ResolveDirectoryConflict(IncomingChangeInfo conflict)
        {
            ConflictResolutionState state;

            if (!mConflictResolutionStates.TryGetValue(conflict.DirectoryConflict, out state))
                return;

            List<DirectoryConflictResolutionData> conflictResolutions =
                new List<DirectoryConflictResolutionData>();

            AddConflictResolution(
                conflict,
                state.ResolveAction,
                state.RenameValue,
                conflictResolutions);

            IncomingChangeInfo metaConflict =
                mIncomingChangesTreeView.GetMetaChange(conflict);

            if (metaConflict != null)
            {
                AddConflictResolution(
                    metaConflict,
                    state.ResolveAction,
                    MetaPath.GetMetaPath(state.RenameValue),
                    conflictResolutions);
            }

            if (state.IsApplyActionsForNextConflictsChecked)
            {
                foreach (IncomingChangeInfo otherConflict in mIncomingChangesTreeView.GetSelectedIncomingChanges())
                {
                    AddConflictResolution(
                        otherConflict,
                        state.ResolveAction,
                        state.RenameValue,
                        conflictResolutions);
                }
            }

            mMergeViewLogic.ResolveDirectoryConflicts(conflictResolutions);
        }

        static void AddConflictResolution(
            IncomingChangeInfo conflict,
            DirectoryConflictResolveActions resolveAction,
            string renameValue,
            List<DirectoryConflictResolutionData> conflictResolutions)
        {
            conflictResolutions.Add(new DirectoryConflictResolutionData(
                conflict.DirectoryConflict,
                conflict.Xlink,
                conflict.GetMount().Mount,
                resolveAction,
                renameValue));
        }

        void DoActionsToolbar(
            bool isProcessMergesButtonVisible,
            bool isCancelMergesButtonVisible,
            bool isProcessMergesButtonEnabled,
            bool isCancelMergesButtonEnabled,
            string processMergesButtonText,
            bool hasPendingDirectoryConflictsCount,
            bool isOperationRunning,
            IWorkspaceWindow workspaceWindow,
            MergeViewLogic mergeViewLogic,
            ProgressControlsForViews.Data progressData)
        {
            GUIStyle guiStyle = new GUIStyle();
            guiStyle.margin = new RectOffset(5, 5, 5, 5);

            EditorGUILayout.BeginHorizontal(guiStyle);

            if (isProcessMergesButtonVisible)
            {
                DoProcessMergesButton(
                    isProcessMergesButtonEnabled && !hasPendingDirectoryConflictsCount,
                    processMergesButtonText,
                    mSwitcher,
                    workspaceWindow,
                    mGuiMessage,
                    mergeViewLogic);
            }

            if (isCancelMergesButtonVisible)
            {
                DoCancelMergesButton(
                    isCancelMergesButtonEnabled,
                    mergeViewLogic);
            }

            if (hasPendingDirectoryConflictsCount)
            {
                GUILayout.Space(5);
                DoWarningMessage();
            }

            if (isOperationRunning)
            {
                DrawProgressForViews.ForIndeterminateProgress(
                    progressData);
            }

            GUILayout.FlexibleSpace();

            DoRefreshButton(
                !isOperationRunning,
                mergeViewLogic);

            EditorGUILayout.EndHorizontal();
        }

        static void DoFileConflictsArea(
            IncomingChangesTreeView incomingChangesTreeView,
            MergeTreeResult conflicts,
            MergeSolvedFileConflicts solvedConflicts,
            MountPointWithPath mount,
            bool isOperationRunning)
        {
            DoConflictsHeader(
                conflicts,
                solvedConflicts,
                mount);

            DoConflictsTree(
                incomingChangesTreeView,
                isOperationRunning);
        }

        static void DoConflictsTree(
            IncomingChangesTreeView incomingChangesTreeView,
            bool isOperationRunning)
        {
            GUI.enabled = !isOperationRunning;

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            incomingChangesTreeView.OnGUI(rect);

            GUI.enabled = true;
        }

        static void DoConflictsHeader(
            MergeTreeResult conflicts,
            MergeSolvedFileConflicts solvedFileConflicts,
            MountPointWithPath mount)
        {
            if (conflicts == null || mount == null)
                return;

            EditorGUILayout.BeginHorizontal();

            DoDirectoryConflictsHeader(conflicts);
            DoFileConflictsHeader(
                conflicts,
                solvedFileConflicts,
                mount);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        static void DoDirectoryConflictsHeader(MergeTreeResult conflicts)
        {
            int directoryConflictsCount =
                MergeTreeResultParser.GetDirectoryConflictsCount(conflicts);
            int pendingDirectoryConflictsCount =
                MergeTreeResultParser.GetUnsolvedDirectoryConflictsCount(
                    conflicts);

            if (directoryConflictsCount == 0)
                return;

            GUIStyle pendingDirectoryConflictsOfTotalStyle =
                pendingDirectoryConflictsCount > 0 ?
                UnityStyles.IncomingChangesTab.RedPendingConflictsOfTotalLabel :
                UnityStyles.IncomingChangesTab.GreenPendingConflictsOfTotalLabel;

            GUILayout.Label(
                string.Format("{0}/{1}", pendingDirectoryConflictsCount, directoryConflictsCount),
                pendingDirectoryConflictsOfTotalStyle);

            GUILayout.Label(
                MergeViewTexts.GetDirectoryConflictsUnsolvedCaption(directoryConflictsCount),
                UnityStyles.IncomingChangesTab.PendingConflictsLabel);
        }

        static void DoFileConflictsHeader(
            MergeTreeResult conflicts,
            MergeSolvedFileConflicts solvedFileConflicts,
            MountPointWithPath mount)
        {
            int fileConflictsCount = MergeTreeResultParser.GetFileConflictsCount(conflicts);
            int pendingFileConflictsCount = MergeTreeResultParser.GetUnsolvedFileConflictsCount(
                conflicts, mount.Id, solvedFileConflicts);

            GUIStyle pendingFileConflictsOfTotalStyle =
                pendingFileConflictsCount > 0 ?
                UnityStyles.IncomingChangesTab.RedPendingConflictsOfTotalLabel :
                UnityStyles.IncomingChangesTab.GreenPendingConflictsOfTotalLabel;

            GUILayout.Label(
                string.Format("{0}/{1}", pendingFileConflictsCount, fileConflictsCount),
                pendingFileConflictsOfTotalStyle);

            GUILayout.Label(
                MergeViewTexts.GetFileConflictsCaption(fileConflictsCount, true),
                UnityStyles.IncomingChangesTab.PendingConflictsLabel);

            GUILayout.Space(5);

            GUILayout.Label(
                MergeViewTexts.GetChangesToApplyCaption(
                    MergeTreeResultParser.GetChangesToApplySummary(conflicts)),
                UnityStyles.IncomingChangesTab.ChangesToApplySummaryLabel);
        }

        static void DoProcessMergesButton(
            bool isEnabled,
            string processMergesButtonText,
            IViewSwitcher switcher,
            IWorkspaceWindow workspaceWindow,
            GuiMessage.IGuiMessage guiMessage,
            MergeViewLogic mergeViewLogic)
        {
            GUI.enabled = isEnabled;

            if (DrawActionButton.For(processMergesButtonText))
            {
                mergeViewLogic.ProcessMerges(
                    workspaceWindow,
                    switcher,
                    guiMessage,
                    new List<string>(),
                    MergeContributorType.MergeContributors,
                    RefreshAsset.UnityAssetDatabase);
            }

            GUI.enabled = true;
        }

        void DoCancelMergesButton(
            bool isEnabled,
            MergeViewLogic mergeViewLogic)
        {
            GUI.enabled = isEnabled;

            if (DrawActionButton.For(PlasticLocalization.GetString(
                    PlasticLocalization.Name.CancelButton)))
            {
                mergeViewLogic.Cancel();

                mIsCancelMergesButtonEnabled = false;
            }

            GUI.enabled = true;
        }

        static void DoWarningMessage()
        {
            string label = PlasticLocalization.GetString(PlasticLocalization.Name.SolveConflictsInLable);

            GUILayout.Label(
                new GUIContent(label, Images.GetWarnIcon()),
                UnityStyles.IncomingChangesTab.HeaderWarningLabel);
        }

        static void DoRefreshButton(
            bool isEnabled,
            MergeViewLogic mergeViewLogic)
        {
            GUI.enabled = isEnabled;

            if (GUILayout.Button(new GUIContent(
                    Images.GetRefreshIcon())))
                mergeViewLogic.Refresh();

            GUI.enabled = true;
        }

        void UpdateFileConflictsTree(
            IncomingChangesTree incomingChangesTree,
            IncomingChangesTreeView incomingChangesTreeView)
        {
            UnityIncomingChangesTree unityIncomingChangesTree = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    unityIncomingChangesTree = UnityIncomingChangesTree.BuildIncomingChangeCategories(
                        incomingChangesTree);
                    incomingChangesTree.ResolveUserNames(
                        new IncomingChangesTree.ResolveUserName());
                },
                /*afterOperationDelegate*/ delegate
                {
                    incomingChangesTreeView.BuildModel(unityIncomingChangesTree);
                    incomingChangesTreeView.Sort();
                    incomingChangesTreeView.Reload();

                    incomingChangesTreeView.SelectFirstUnsolvedDirectoryConflict();
                });
        }

        static void DoInfoMessageArea(string message)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.HelpBox(message, MessageType.Info);

            EditorGUILayout.EndHorizontal();
        }

        static void DoErrorMessageArea(string message)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.HelpBox(message, MessageType.Error);

            EditorGUILayout.EndHorizontal();
        }

        void BuildComponents(WorkspaceInfo wkInfo)
        {
            IncomingChangesTreeHeaderState incomingChangesHeaderState =
                IncomingChangesTreeHeaderState.GetDefault();

            TreeHeaderSettings.Load(incomingChangesHeaderState,
                UnityConstants.DEVELOPER_INCOMING_CHANGES_TABLE_SETTINGS_NAME,
                (int)IncomingChangesTreeColumn.Path, true);

            mIncomingChangesTreeView = new IncomingChangesTreeView(
                wkInfo, incomingChangesHeaderState,
                IncomingChangesTreeHeaderState.GetColumnNames(),
                new IncomingChangesViewMenu(this, this));

            mIncomingChangesTreeView.Reload();
        }

        static ConflictResolutionState GetConflictResolutionState(
            DirectoryConflict directoryConflict,
            DirectoryConflictAction[] conflictActions,
            Dictionary<DirectoryConflict, ConflictResolutionState> conflictResoltionStates)
        {
            ConflictResolutionState result;

            if (conflictResoltionStates.TryGetValue(directoryConflict, out result))
                return result;

            result = ConflictResolutionState.Build(directoryConflict, conflictActions);

            conflictResoltionStates.Add(directoryConflict, result);
            return result;
        }

        static int GetPendingConflictsCount(
            List<IncomingChangeInfo> selectedChangeInfos)
        {
            int result = 0;
            foreach (IncomingChangeInfo changeInfo in selectedChangeInfos)
            {
                if (changeInfo.DirectoryConflict.IsResolved())
                    continue;

                result++;
            }

            return result;
        }
        bool mIsProcessMergesButtonVisible;
        bool mIsCancelMergesButtonVisible;
        bool mIsMessageLabelVisible;
        bool mIsErrorMessageLabelVisible;

        bool mIsProcessMergesButtonEnabled;
        bool mIsCancelMergesButtonEnabled;
        bool mHasPendingDirectoryConflicts;
        bool mIsOperationRunning;

        string mProcessMergesButtonText;
        string mMessageLabelText;
        string mErrorMessageLabelText;

        IncomingChangesTreeView mIncomingChangesTreeView;

        MergeTreeResult mResultConflicts;
        MergeSolvedFileConflicts mSolvedFileConflicts;
        MountPointWithPath mRootMountPoint;

        Dictionary<DirectoryConflict, ConflictResolutionState> mConflictResolutionStates =
            new Dictionary<DirectoryConflict, ConflictResolutionState>();

        readonly ProgressControlsForViews mProgressControls;
        readonly MergeViewLogic mMergeViewLogic;
        readonly MergeController mMergeController;
        readonly GuiMessage.IGuiMessage mGuiMessage;
        readonly EditorWindow mParentWindow;
        readonly NewIncomingChangesUpdater mNewIncomingChangesUpdater;
        readonly IViewSwitcher mSwitcher;
        readonly IWorkspaceWindow mWorkspaceWindow;
        readonly WorkspaceInfo mWkInfo;
    }
}