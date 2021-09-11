using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Codice.Client.Common;
using Codice.Client.Common.FsNodeReaders;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using Codice.CM.Common.Merge;
using Codice.LogWrapper;
using GluonGui;
using GluonGui.WorkspaceWindow.Views.Checkin.Operations;

using PlasticGui;
using PlasticGui.Help.Actions;
using PlasticGui.Help.Conditions;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Diff;
using PlasticGui.WorkspaceWindow.Items;
using PlasticGui.WorkspaceWindow.Open;
using PlasticGui.WorkspaceWindow.PendingChanges;
using Unity.PlasticSCM.Editor.AssetsOverlays;

using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.Help;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.Views.PendingChanges.Dialogs;
using Unity.PlasticSCM.Editor.Views.PendingChanges.PendingMergeLinks;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.Tool;

using GluonNewIncomingChangesUpdater = PlasticGui.Gluon.WorkspaceWindow.NewIncomingChangesUpdater;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges
{
    internal partial class PendingChangesTab :
        IRefreshableView,
        IPendingChangesView,
        CheckinUIOperation.ICheckinView,
        PendingChangesViewMenu.IMetaMenuOperations,
        IPendingChangesMenuOperations,
        IOpenMenuOperations,
        IFilesFilterPatternsMenuOperations,
        PendingChangesOptionsDialog.IAutorefreshView
    {
        internal IProgressControls ProgressControlsForTesting { get { return mProgressControls; } }
        internal HelpPanel HelpPanelForTesting { get { return mHelpPanel; } }

        internal void SetMergeLinksForTesting(
            IDictionary<MountPoint, IList<PendingMergeLink>> mergeLinks)
        {
            mPendingMergeLinks = mergeLinks;

            UpdateMergeLinksList();
        }

        internal string CommentText { get; set; }

        internal bool ForceToShowComment { get; set; }

        internal PendingChangesTab(
            WorkspaceInfo wkInfo,
            ViewHost viewHost,
            bool isGluonMode,
            WorkspaceWindow workspaceWindow,
            IViewSwitcher switcher,
            IMergeViewLauncher mergeViewLauncher,
            IHistoryViewLauncher historyViewLauncher,
            PlasticGui.WorkspaceWindow.PendingChanges.PendingChanges pendingChanges,
            NewIncomingChangesUpdater developerNewIncomingChangesUpdater,
            GluonNewIncomingChangesUpdater gluonNewIncomingChangesUpdater,
            IAssetStatusCache assetStatusCache,
            EditorWindow parentWindow)
        {
            mWkInfo = wkInfo;
            mViewHost = viewHost;
            mIsGluonMode = isGluonMode;
            mWorkspaceWindow = workspaceWindow;
            mHistoryViewLauncher = historyViewLauncher;
            mPendingChanges = pendingChanges;
            mDeveloperNewIncomingChangesUpdater = developerNewIncomingChangesUpdater;
            mGluonNewIncomingChangesUpdater = gluonNewIncomingChangesUpdater;
            mAssetStatusCache = assetStatusCache;
            mParentWindow = parentWindow;
            mGuiMessage = new UnityPlasticGuiMessage(parentWindow);

            mCheckedStateManager = new CheckedStateManager();

            mNewChangesInWk = NewChangesInWk.Build(
                mWkInfo, new BuildWorkspacekIsRelevantNewChange());

            BuildComponents(isGluonMode, parentWindow);

            mProgressControls = new ProgressControlsForViews();

            workspaceWindow.RegisterPendingChangesProgressControls(
                mProgressControls);

            mPendingChangesOperations = new PendingChangesOperations(
                mWkInfo, workspaceWindow, switcher, mergeViewLauncher, this,
                mProgressControls, workspaceWindow, null, null, null);

            InitIgnoreRulesAndRefreshView(mWkInfo.ClientPath, this);
        }

        internal void AutoRefresh()
        {
            if (mIsAutoRefreshDisabled)
                return;

            if (!PlasticGuiConfig.Get().Configuration.CommitAutoRefresh)
                return;

            if (mIsRefreshing || mWorkspaceWindow.IsOperationInProgress())
                return;

            if (mNewChangesInWk != null && !mNewChangesInWk.Detected())
                return;

            ((IRefreshableView)this).Refresh();
        }

        internal void ClearIsCommentWarningNeeded()
        {
            mIsCommentWarningNeeded = false;
        }

        internal void UpdateIsCommentWarningNeeded(string comment)
        {
            mIsCommentWarningNeeded = string.IsNullOrEmpty(comment)
                && mPendingChanges.HasPendingChanges();
        }

        internal void OnDisable()
        {
            mSearchField.downOrUpArrowKeyPressed -=
                SearchField_OnDownOrUpArrowKeyPressed;

            TreeHeaderSettings.Save(
                mPendingChangesTreeView.multiColumnHeader.state,
                UnityConstants.PENDING_CHANGES_TABLE_SETTINGS_NAME);
        }

        internal void Update()
        {
            mProgressControls.UpdateProgress(mParentWindow);
        }

        internal void OnGUI()
        {
            DrawCommentTextArea.For(
                this,
                mParentWindow.position.width,
                mProgressControls.IsOperationRunning());

            DoOperationsToolbar(
                mWkInfo,
                mIsGluonMode,
                mAdvancedDropdownMenu,
                mProgressControls.IsOperationRunning());

            if (!string.IsNullOrEmpty(mGluonWarningMessage))
                DoWarningMessage(mGluonWarningMessage);

            DoActionsToolbar(
                mWkInfo,
                mIsGluonMode,
                this,
                mProgressControls,
                mParentWindow);

            DoChangesArea(
                mPendingChangesTreeView,
                mProgressControls.IsOperationRunning());

            if (HasPendingMergeLinks())
                DoMergeLinksArea(mMergeLinksListView, mParentWindow.position.width);

            if (mProgressControls.HasNotification())
            {
                DrawProgressForViews.ForNotificationArea(
                    mProgressControls.ProgressData);
            }

            DrawHelpPanel.For(mHelpPanel);
        }

        void IPendingChangesView.ClearComments()
        {
            ClearComments();
        }

        void IRefreshableView.Refresh()
        {
            if (!mAreIgnoreRulesInitialized)
                return;

            DrawAssetOverlay.ClearCache();

            if (mDeveloperNewIncomingChangesUpdater != null)
                mDeveloperNewIncomingChangesUpdater.Update();

            if (mGluonNewIncomingChangesUpdater != null)
                mGluonNewIncomingChangesUpdater.Update(DateTime.Now);

            FillPendingChanges(mNewChangesInWk);
        }

        void IPendingChangesView.ClearChangesToCheck(List<string> changes)
        {
            mCheckedStateManager.ClearChangesToCheck(changes);

            mParentWindow.Repaint();
        }

        void IPendingChangesView.CleanCheckedElements(List<ChangeInfo> checkedChanges)
        {
            mCheckedStateManager.Clean(checkedChanges);

            mParentWindow.Repaint();
        }

        void IPendingChangesView.CheckChanges(List<string> changesToCheck)
        {
            mCheckedStateManager.SetChangesToCheck(changesToCheck);

            mParentWindow.Repaint();
        }

        bool IPendingChangesView.IncludeDependencies(
            IList<ChangeDependencies<ChangeInfo>> changesDependencies,
            string operation)
        {
            return DependenciesDialog.IncludeDependencies(
                mWkInfo, changesDependencies, operation, mParentWindow);
        }

        CheckinMergeNeededData IPendingChangesView.CheckinMergeNeeded()
        {
            return CheckinMergeNeededDialog.Merge(mWkInfo, mParentWindow);
        }

        SearchMatchesData IPendingChangesView.AskForMatches(string changePath)
        {
            throw new NotImplementedException();
        }

        void IPendingChangesView.CleanLinkedTasks()
        {
        }

        void PendingChangesOptionsDialog.IAutorefreshView.DisableAutorefresh()
        {
            mIsAutoRefreshDisabled = true;
        }

        void CheckinUIOperation.ICheckinView.CollapseWarningMessagePanel()
        {
            mGluonWarningMessage = string.Empty;

            mParentWindow.Repaint();
        }

        void CheckinUIOperation.ICheckinView.ExpandWarningMessagePanel(string text)
        {
            mGluonWarningMessage = text;

            mParentWindow.Repaint();
        }

        void PendingChangesOptionsDialog.IAutorefreshView.EnableAutorefresh()
        {
            mIsAutoRefreshDisabled = false;
        }

        void CheckinUIOperation.ICheckinView.ClearComments()
        {
            ClearComments();
        }

        bool PendingChangesViewMenu.IMetaMenuOperations.SelectionHasMeta()
        {
            return mPendingChangesTreeView.SelectionHasMeta();
        }

        void PendingChangesViewMenu.IMetaMenuOperations.DiffMeta()
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(mIsGluonMode))
                return;

            ChangeInfo selectedChange = PendingChangesSelection
                .GetSelectedChange(mPendingChangesTreeView);
            ChangeInfo selectedChangeMeta = mPendingChangesTreeView.GetMetaChange(
                selectedChange);

            ChangeInfo changedForMoved = mPendingChanges.GetChangedForMoved(selectedChange);
            ChangeInfo changedForMovedMeta = (changedForMoved == null) ?
                null : mPendingChangesTreeView.GetMetaChange(changedForMoved);

            DiffOperation.DiffWorkspaceContent(
                mWkInfo,
                selectedChangeMeta,
                changedForMovedMeta,
                mProgressControls,
                null, null);
        }

        void PendingChangesViewMenu.IMetaMenuOperations.HistoryMeta()
        {
            ChangeInfo selectedChange = PendingChangesSelection
                .GetSelectedChange(mPendingChangesTreeView);
            ChangeInfo selectedChangeMeta = mPendingChangesTreeView.GetMetaChange(
                selectedChange);

            mHistoryViewLauncher.ShowHistoryView(
                selectedChangeMeta.RepositorySpec,
                selectedChangeMeta.RevInfo.ItemId,
                selectedChangeMeta.Path,
                selectedChangeMeta.IsDirectory);
        }

        void PendingChangesViewMenu.IMetaMenuOperations.OpenMeta()
        {
            List<string> selectedPaths = PendingChangesSelection
                .GetSelectedMetaPaths(mPendingChangesTreeView);

            FileSystemOperation.Open(selectedPaths);
        }

        void PendingChangesViewMenu.IMetaMenuOperations.OpenMetaWith()
        {
            List<string> selectedPaths = PendingChangesSelection
                .GetSelectedMetaPaths(mPendingChangesTreeView);

            OpenOperation.OpenWith(
                FileSystemOperation.GetExePath(),
                selectedPaths);
        }

        void PendingChangesViewMenu.IMetaMenuOperations.OpenMetaInExplorer()
        {
            List<string> selectedPaths = PendingChangesSelection
                .GetSelectedMetaPaths(mPendingChangesTreeView);

            if (selectedPaths.Count < 1)
                return;

            FileSystemOperation.OpenInExplorer(selectedPaths[0]);
        }

        SelectedChangesGroupInfo IPendingChangesMenuOperations.GetSelectedChangesGroupInfo()
        {
            return PendingChangesSelection.GetSelectedChangesGroupInfo(
                mPendingChangesTreeView);
        }

        void IPendingChangesMenuOperations.Diff()
        {
            if (LaunchTool.ShowDownloadPlasticExeWindow(mIsGluonMode))
                return;

            ChangeInfo selectedChange = PendingChangesSelection
                .GetSelectedChange(mPendingChangesTreeView);

            DiffOperation.DiffWorkspaceContent(
                mWkInfo,
                selectedChange,
                mPendingChanges.GetChangedForMoved(selectedChange),
                null,
                null,
                null);
        }

        void IPendingChangesMenuOperations.UndoChanges()
        {
            List<ChangeInfo> changesToUndo = PendingChangesSelection
                .GetSelectedChanges(mPendingChangesTreeView);

            List<ChangeInfo> dependenciesCandidates =
                mPendingChangesTreeView.GetDependenciesCandidates(changesToUndo, true);

            UndoChangesForMode(mIsGluonMode, changesToUndo, dependenciesCandidates);
        }

        void IPendingChangesMenuOperations.SearchMatches()
        {
            ChangeInfo selectedChange = PendingChangesSelection
                .GetSelectedChange(mPendingChangesTreeView);

            if (selectedChange == null)
                return;

            SearchMatchesOperation operation = new SearchMatchesOperation(
                mWkInfo, mWorkspaceWindow, this,
                mProgressControls, mDeveloperNewIncomingChangesUpdater);

            operation.SearchMatches(
                selectedChange,
                PendingChangesSelection.GetAllChanges(mPendingChangesTreeView));
        }

        void IPendingChangesMenuOperations.ApplyLocalChanges()
        {
            List<ChangeInfo> selectedChanges = PendingChangesSelection
                .GetSelectedChanges(mPendingChangesTreeView);

            if (selectedChanges.Count == 0)
                return;

            ApplyLocalChangesOperation operation = new ApplyLocalChangesOperation(
                mWkInfo, mWorkspaceWindow, this,
                mProgressControls, mDeveloperNewIncomingChangesUpdater);

            operation.ApplyLocalChanges(
                selectedChanges,
                PendingChangesSelection.GetAllChanges(mPendingChangesTreeView));
        }

        void IPendingChangesMenuOperations.Delete()
        {
            List<string> privateDirectoriesToDelete;
            List<string> privateFilesToDelete;

            if (!mPendingChangesTreeView.GetSelectedPathsToDelete(
                    out privateDirectoriesToDelete,
                    out privateFilesToDelete))
                return;

            DeleteOperation.Delete(
                mWorkspaceWindow, mProgressControls,
                privateDirectoriesToDelete, privateFilesToDelete,
                mDeveloperNewIncomingChangesUpdater,
                RefreshAsset.UnityAssetDatabase);
        }

        void IPendingChangesMenuOperations.Annotate()
        {
            throw new NotImplementedException();
        }

        void IPendingChangesMenuOperations.History()
        {
            ChangeInfo selectedChange = PendingChangesSelection.
                GetSelectedChange(mPendingChangesTreeView);

            mHistoryViewLauncher.ShowHistoryView(
                selectedChange.RepositorySpec,
                selectedChange.RevInfo.ItemId,
                selectedChange.Path,
                selectedChange.IsDirectory);
        }

        void IOpenMenuOperations.Open()
        {
            List<string> selectedPaths = PendingChangesSelection.
                GetSelectedPathsWithoutMeta(mPendingChangesTreeView);

            FileSystemOperation.Open(selectedPaths);
        }

        void IOpenMenuOperations.OpenWith()
        {
            List<string> selectedPaths = PendingChangesSelection.
                GetSelectedPathsWithoutMeta(mPendingChangesTreeView);

            OpenOperation.OpenWith(
                FileSystemOperation.GetExePath(),
                selectedPaths);
        }

        void IOpenMenuOperations.OpenWithCustom(string exePath, string args)
        {
            List<string> selectedPaths = PendingChangesSelection.
                GetSelectedPathsWithoutMeta(mPendingChangesTreeView);

            OpenOperation.OpenWith(exePath, selectedPaths);
        }

        void IOpenMenuOperations.OpenInExplorer()
        {
            List<string> selectedPaths = PendingChangesSelection
                .GetSelectedPathsWithoutMeta(mPendingChangesTreeView);

            if (selectedPaths.Count < 1)
                return;

            FileSystemOperation.OpenInExplorer(selectedPaths[0]);
        }

        void IFilesFilterPatternsMenuOperations.AddFilesFilterPatterns(
            FilterTypes type, FilterActions action, FilterOperationType operation)
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedPaths(
                mPendingChangesTreeView);

            string[] rules = FilterRulesGenerator.GenerateRules(
                selectedPaths, mWkInfo.ClientPath, action, operation);

            bool isApplicableToAllWorkspaces = !mIsGluonMode;
            bool isAddOperation = operation == FilterOperationType.Add;

            FilterRulesConfirmationData filterRulesConfirmationData = 
                FilterRulesConfirmationDialog.AskForConfirmation(
                    rules, isAddOperation, isApplicableToAllWorkspaces, mParentWindow);

            AddFilesFilterPatternsOperation.Run(
                mWkInfo, mWorkspaceWindow, type, operation, filterRulesConfirmationData);
        }

        void SearchField_OnDownOrUpArrowKeyPressed()
        {
            mPendingChangesTreeView.SetFocusAndEnsureSelectedItem();
        }

        void InitIgnoreRulesAndRefreshView(
            string wkPath, IRefreshableView view)
        {
            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    if (IsIgnoreConfigDefined.For(wkPath))
                        return;

                    AddIgnoreRules.WriteRules(
                        wkPath, UnityConditions.GetMissingIgnoredRules(wkPath));
                },
                /*afterOperationDelegate*/ delegate
                {
                    mAreIgnoreRulesInitialized = true;

                    view.Refresh();

                    if (waiter.Exception == null)
                        return;

                    mLog.ErrorFormat(
                        "Error adding ignore rules for Unity: {0}",
                        waiter.Exception);

                    mLog.DebugFormat(
                        "Stack trace: {0}",
                        waiter.Exception.StackTrace);
                });
        }

        void FillPendingChanges(INewChangesInWk newChangesInWk)
        {
            if (mIsRefreshing)
                return;

            mIsRefreshing = true;

            List<ChangeInfo> changesToSelect =
                PendingChangesSelection.GetChangesToFocus(mPendingChangesTreeView);

            ((IProgressControls)mProgressControls).ShowProgress(PlasticLocalization.
                GetString(PlasticLocalization.Name.LoadingPendingChanges));

            IDictionary<MountPoint, IList<PendingMergeLink>> mergeLinks = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter();
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    FilterManager.Get().Reload();

                    WorkspaceStatusOptions options = WorkspaceStatusOptions.None;
                    options |= WorkspaceStatusOptions.FindAdded;
                    options |= WorkspaceStatusOptions.FindDeleted;
                    options |= WorkspaceStatusOptions.FindMoved;
                    options |= WorkspaceStatusOptions.SplitModifiedMoved;
                    options |= PendingChangesOptions.GetWorkspaceStatusOptions();

                    if (newChangesInWk != null)
                        newChangesInWk.Detected();

                    mPendingChanges.Calculate(
                        options, PendingChangesOptions.GetMovedMatchingOptions());

                    mergeLinks = Plastic.API.GetPendingMergeLinks(mWkInfo);
                },
                /*afterOperationDelegate*/ delegate
                {
                    mPendingMergeLinks = mergeLinks;

                    try
                    {
                        if (waiter.Exception != null)
                        {
                            ExceptionsHandler.DisplayException(waiter.Exception);
                            return;
                        }

                        UpdateChangesTree();

                        UpdateMergeLinksList();

                        PendingChangesSelection.SelectChanges(
                            mPendingChangesTreeView, changesToSelect);
                    }
                    finally
                    {
                        ((IProgressControls)mProgressControls).HideProgress();

                        //UpdateIsCommentWarningNeeded(CommentText);

                        UpdateNotificationPanel();

                        mIsRefreshing = false;
                    }
                });
        }

        void DoOperationsToolbar(
            WorkspaceInfo wkInfo,
            bool isGluonMode,
            GenericMenu advancedDropdownMenu,
            bool isOperationRunning)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUISpace.ForToolbar();

            using (new GuiEnabled(!isOperationRunning))
            {
                if (DrawActionButton.For(PlasticLocalization.GetString(
                        PlasticLocalization.Name.CheckinChanges)))
                {
                    UpdateIsCommentWarningNeeded(CommentText);

                    if (!mIsCommentWarningNeeded)
                        CheckinForMode(wkInfo, isGluonMode, mKeepItemsLocked);
                }

                if (DrawActionButton.For(PlasticLocalization.GetString(
                        PlasticLocalization.Name.UndoChanges)))
                {
                    UndoForMode(wkInfo, isGluonMode);
                }

                if (isGluonMode)
                {
                    mKeepItemsLocked = EditorGUILayout.ToggleLeft(
                        PlasticLocalization.GetString(PlasticLocalization.Name.KeepLocked),
                        mKeepItemsLocked,
                        GUILayout.Width(UnityConstants.EXTRA_LARGE_BUTTON_WIDTH));
                }
                //TODO: Codice - beta: hide the advanced menu until the behavior is implemented
                /*else
                {
                    var dropDownContent = new GUIContent(string.Empty);
                    var dropDownRect = GUILayoutUtility.GetRect(
                        dropDownContent, EditorStyles.toolbarDropDown);

                    if (EditorGUI.DropdownButton(dropDownRect, dropDownContent,
                            FocusType.Passive, EditorStyles.toolbarDropDown))
                        advancedDropdownMenu.DropDown(dropDownRect);
                }*/
            }

            if (mIsCommentWarningNeeded)
            {
                GUILayout.Space(5);
                DoCheckinWarningMessage();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        void UpdateChangesTree()
        {
            mPendingChangesTreeView.BuildModel(
                mPendingChanges, mCheckedStateManager);

            mPendingChangesTreeView.Refilter();

            mPendingChangesTreeView.Sort();

            mPendingChangesTreeView.Reload();
        }

        static void DoWarningMessage(string message)
        {
            GUILayout.Label(message, UnityStyles.WarningMessage);
        }

        void UpdateMergeLinksList()
        {
            mMergeLinksListView.BuildModel(mPendingMergeLinks);

            mMergeLinksListView.Reload();
        }

        void UpdateNotificationPanel()
        {
            if (Plastic.API.IsFsReaderWatchLimitReached(mWkInfo))
            {
                ((IProgressControls)mProgressControls).ShowWarning(PlasticLocalization.
                    GetString(PlasticLocalization.Name.NotifyLinuxWatchLimitWarning));
                return;
            }
        }

        static void DoCheckinWarningMessage()
        {
            string label = PlasticLocalization.GetString(PlasticLocalization.Name.PleaseComment);

            GUILayout.Label(
                new GUIContent(label, Images.GetWarnIcon()),
                UnityStyles.PendingChangesTab.CommentWarningIcon);
        }

        void DoActionsToolbar(
            WorkspaceInfo workspaceInfo,
            bool isGluonMode,
            IRefreshableView refreshableView,
            ProgressControlsForViews progressControls,
            EditorWindow editorWindow)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            string checkinHeader = string.Format(
                PlasticLocalization.GetString(PlasticLocalization.Name.CheckinHeader),
                mPendingChangesTreeView.GetSelectedItemCount(),
                mPendingChangesTreeView.GetTotalItemCount());

            GUILayout.Label(checkinHeader, UnityStyles.PendingChangesTab.HeaderLabel);

            if (progressControls.IsOperationRunning())
            {
                DrawProgressForViews.ForIndeterminateProgress(
                    progressControls.ProgressData);
            }

            GUILayout.FlexibleSpace();

            DrawSearchField.For(
                mSearchField,
                mPendingChangesTreeView,
                UnityConstants.SEARCH_FIELD_WIDTH);

            if (GUILayout.Button(PlasticLocalization.GetString(
                    PlasticLocalization.Name.Options),
                    EditorStyles.toolbarButton,
                    GUILayout.Width(UnityConstants.REGULAR_BUTTON_WIDTH)))
            {
                PendingChangesOptionsDialog.ChangeOptions(
                    workspaceInfo, refreshableView, this, editorWindow);
                GUIUtility.ExitGUI();
            }

            DoRefreshButton(
                refreshableView,
                progressControls.IsOperationRunning());

            EditorGUILayout.EndHorizontal();
        }

        static void DoChangesArea(
            PendingChangesTreeView changesTreeView,
            bool isOperationRunning)
        {
            GUI.enabled = !isOperationRunning;

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            changesTreeView.OnGUI(rect);

            GUI.enabled = true;
        }

        bool HasPendingMergeLinks()
        {
            if (mPendingMergeLinks == null)
                return false;

            return mPendingMergeLinks.Count > 0;
        }

        static void DoMergeLinksArea(
            MergeLinksListView mergeLinksListView, float width)
        {
            GUILayout.Label(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.MergeLinkDescriptionColumn),
                EditorStyles.boldLabel);

            float desiredTreeHeight = mergeLinksListView.DesiredHeight;

            Rect treeRect = GUILayoutUtility.GetRect(
                0, width, desiredTreeHeight, desiredTreeHeight);

            mergeLinksListView.OnGUI(treeRect);
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

        void BuildComponents(
            bool isGluonMode,
            EditorWindow plasticWindow)
        {
            mHelpPanel = new HelpPanel(plasticWindow);

            mAdvancedDropdownMenu = new GenericMenu();
            mAdvancedDropdownMenu.AddItem(new GUIContent(
                PlasticLocalization.GetString(PlasticLocalization.Name.UndoUnchangedButton)),
                false, () => { });

            mSearchField = new SearchField();
            mSearchField.downOrUpArrowKeyPressed += SearchField_OnDownOrUpArrowKeyPressed;

            PendingChangesTreeHeaderState headerState =
                PendingChangesTreeHeaderState.GetDefault(isGluonMode);
            TreeHeaderSettings.Load(headerState,
                UnityConstants.PENDING_CHANGES_TABLE_SETTINGS_NAME,
                (int)PendingChangesTreeColumn.Item, true);

            mPendingChangesTreeView = new PendingChangesTreeView(
                mWkInfo, mIsGluonMode, headerState,
                PendingChangesTreeHeaderState.GetColumnNames(),
                new PendingChangesViewMenu(this, this, this, this, mIsGluonMode),
                mAssetStatusCache);
            mPendingChangesTreeView.Reload();

            mMergeLinksListView = new MergeLinksListView();
            mMergeLinksListView.Reload();
        }
        INewChangesInWk mNewChangesInWk;
        GenericMenu mAdvancedDropdownMenu;

        void ClearComments()
        {
            CommentText = string.Empty;
            ForceToShowComment = true;

            mParentWindow.Repaint();
        }

        PendingChangesTreeView mPendingChangesTreeView;
        MergeLinksListView mMergeLinksListView;
        HelpPanel mHelpPanel;

        volatile bool mAreIgnoreRulesInitialized = false;
        bool mIsRefreshing;

        bool mIsAutoRefreshDisabled;
        bool mIsCommentWarningNeeded = false;
        bool mKeepItemsLocked;
        string mGluonWarningMessage;

        IDictionary<MountPoint, IList<PendingMergeLink>> mPendingMergeLinks;

        SearchField mSearchField;

        readonly ProgressControlsForViews mProgressControls;
        readonly EditorWindow mParentWindow;
        readonly IAssetStatusCache mAssetStatusCache;

        readonly PendingChangesOperations mPendingChangesOperations;
        readonly CheckedStateManager mCheckedStateManager;
        readonly GuiMessage.IGuiMessage mGuiMessage;
        readonly NewIncomingChangesUpdater mDeveloperNewIncomingChangesUpdater;
        readonly GluonNewIncomingChangesUpdater mGluonNewIncomingChangesUpdater;
        readonly bool mIsGluonMode;
        readonly IHistoryViewLauncher mHistoryViewLauncher;
        readonly PlasticGui.WorkspaceWindow.PendingChanges.PendingChanges mPendingChanges;
        readonly WorkspaceWindow mWorkspaceWindow;
        readonly ViewHost mViewHost;
        readonly WorkspaceInfo mWkInfo;

        static readonly ILog mLog = LogManager.GetLogger("PendingChangesTab");
    }
}