using System;

using UnityEditor;

using Codice.Client.Common;

using Codice.CM.Common;
using GluonGui;
using PlasticGui;
using PlasticGui.Gluon;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Merge;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.Views.Changesets;
using Unity.PlasticSCM.Editor.Views.History;
using Unity.PlasticSCM.Editor.Views.IncomingChanges;
using Unity.PlasticSCM.Editor.Views.PendingChanges;

using GluonNewIncomingChangesUpdater = PlasticGui.Gluon.WorkspaceWindow.NewIncomingChangesUpdater;
using ObjectInfo = Codice.CM.Common.ObjectInfo;

namespace Unity.PlasticSCM.Editor
{
    internal class ViewSwitcher :
        IViewSwitcher,
        IMergeViewLauncher,
        IGluonViewSwitcher,
        IHistoryViewLauncher
    {
        internal PendingChangesTab PendingChangesTabForTesting { get { return mPendingChangesTab; } }
        internal IIncomingChangesTab IncomingChangesTabForTesting { get { return mIncomingChangesTab; } }

        internal ViewSwitcher(
            WorkspaceInfo wkInfo,
            ViewHost viewHost,
            bool isGluonMode,
            PlasticGui.WorkspaceWindow.PendingChanges.PendingChanges pendingChanges,
            NewIncomingChangesUpdater developerNewIncomingChangesUpdater,
            GluonNewIncomingChangesUpdater gluonNewIncomingChangesUpdater,
            IIncomingChangesNotificationPanel incomingChangesNotificationPanel,
            IAssetStatusCache assetStatusCache,
            EditorWindow parentWindow)
        {
            mWkInfo = wkInfo;
            mViewHost = viewHost;
            mIsGluonMode = isGluonMode;
            mPendingChanges = pendingChanges;
            mDeveloperNewIncomingChangesUpdater = developerNewIncomingChangesUpdater;
            mGluonNewIncomingChangesUpdater = gluonNewIncomingChangesUpdater;
            mIncomingChangesNotificationPanel = incomingChangesNotificationPanel;
            mAssetStatusCache = assetStatusCache;
            mParentWindow = parentWindow;

            mPendingChangesTabButton = new TabButton();
            mIncomingChangesTabButton = new TabButton();
            mChangesetsTabButton = new TabButton();
            mHistoryTabButton = new TabButton();
        }

        internal void SetWorkspaceWindow(WorkspaceWindow workspaceWindow)
        {
            mWorkspaceWindow = workspaceWindow;
        }

        internal void ShowInitialView()
        {
            ShowPendingChangesView();
        }

        internal void AutoRefreshPendingChangesView()
        {
            AutoRefresh.PendingChangesView(
                mPendingChangesTab);
        }

        internal void AutoRefreshIncomingChangesView()
        {
            AutoRefresh.IncomingChangesView(
                mIncomingChangesTab);
        }

        internal void RefreshView(ViewType viewType)
        {
            IRefreshableView view = GetRefreshableView(viewType);

            if (view == null)
                return;

            view.Refresh();
        }

        internal void OnDisable()
        {
            PlasticAssetsProcessor.UnRegisterViews();

            if (mPendingChangesTab != null)
                mPendingChangesTab.OnDisable();

            if (mIncomingChangesTab != null)
                mIncomingChangesTab.OnDisable();

            if (mChangesetsTab != null)
                mChangesetsTab.OnDisable();

            if (mHistoryTab != null)
                mHistoryTab.OnDisable();
        }

        internal void Update()
        {
            if (IsViewSelected(SelectedTab.PendingChanges))
            {
                mPendingChangesTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.IncomingChanges))
            {
                mIncomingChangesTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.Changesets))
            {
                mChangesetsTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.History))
            {
                mHistoryTab.Update();
                return;
            }
        }

        internal void TabButtonsGUI()
        {
            InitializeTabButtonWidth();

            PendingChangesTabButtonGUI();

            IncomingChangesTabButtonGUI();

            ChangesetsTabButtonGUI();

            HistoryTabButtonGUI();
        }

        internal void TabViewGUI()
        {
            if (IsViewSelected(SelectedTab.PendingChanges))
            {
                mPendingChangesTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.IncomingChanges))
            {
                mIncomingChangesTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.Changesets))
            {
                mChangesetsTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.History))
            {
                mHistoryTab.OnGUI();
                return;
            }
        }

        void IViewSwitcher.ShowPendingChanges()
        {
            ShowPendingChangesView();
            mParentWindow.Repaint();
        }

        void IViewSwitcher.ShowSyncView(string syncViewToSelect)
        {
            throw new NotImplementedException();
        }

        void IViewSwitcher.ShowBranchExplorerView()
        {
            //TODO: Codice
            //launch plastic with branch explorer view option
        }

        void IViewSwitcher.DisableMergeView()
        {
        }

        bool IViewSwitcher.IsIncomingChangesView()
        {
            return IsViewSelected(SelectedTab.IncomingChanges);
        }

        void IViewSwitcher.CloseIncomingChangesView()
        {
            ((IViewSwitcher)this).DisableMergeView();
        }

        void IMergeViewLauncher.MergeFrom(ObjectInfo objectInfo, EnumMergeType mergeType)
        {
            ((IMergeViewLauncher)this).MergeFromInterval(objectInfo, null, mergeType);
        }

        void IMergeViewLauncher.MergeFrom(ObjectInfo objectInfo, EnumMergeType mergeType, ShowIncomingChangesFrom from)
        {
            ((IMergeViewLauncher)this).MergeFromInterval(objectInfo, null, mergeType);
        }

        void IMergeViewLauncher.MergeFromInterval(ObjectInfo objectInfo, ObjectInfo ancestorChangesetInfo, EnumMergeType mergeType)
        {
            if (mergeType == EnumMergeType.IncomingMerge)
            {
                ShowIncomingChangesView();
                mParentWindow.Repaint();
                return;
            }

            LaunchTool.OpenMerge(mWkInfo.ClientPath, mIsGluonMode);
        }

        void IGluonViewSwitcher.ShowIncomingChangesView()
        {
            ShowIncomingChangesView();
            mParentWindow.Repaint();
        }

        void IHistoryViewLauncher.ShowHistoryView(
            RepositorySpec repSpec,
            long itemId,
            string path,
            bool isDirectory)
        {
            ShowHistoryView(
                repSpec,
                itemId,
                path,
                isDirectory);

            mParentWindow.Repaint();
        }

        void CloseHistoryTab()
        {
            ShowView(mPreviousSelectedTab);

            mViewHost.RemoveRefreshableView(
                ViewType.HistoryView, mHistoryTab);

            mHistoryTab.OnDisable();
            mHistoryTab = null;

            mParentWindow.Repaint();
        }

        void ShowPendingChangesView()
        {
            if (mPendingChangesTab == null)
            {
                mPendingChangesTab = new PendingChangesTab(
                    mWkInfo,
                    mViewHost,
                    mIsGluonMode,
                    mWorkspaceWindow,
                    this,
                    this,
                    this,
                    mPendingChanges,
                    mDeveloperNewIncomingChangesUpdater,
                    mGluonNewIncomingChangesUpdater,
                    mAssetStatusCache,
                    mParentWindow);

                mViewHost.AddRefreshableView(
                    ViewType.CheckinView,
                    mPendingChangesTab);

                PlasticAssetsProcessor.
                    RegisterPendingChangesView(mPendingChangesTab);
            }

            bool wasPendingChangesSelected =
                IsViewSelected(SelectedTab.PendingChanges);

            if (!wasPendingChangesSelected)
            {
                mPendingChangesTab.AutoRefresh();
            }

            SetSelectedView(SelectedTab.PendingChanges);
        }

        void ShowIncomingChangesView()
        {
            if (mIncomingChangesTab == null)
            {
                mIncomingChangesTab = mIsGluonMode ?
                    new Views.IncomingChanges.Gluon.IncomingChangesTab(
                        mWkInfo,
                        mViewHost,
                        mWorkspaceWindow,
                        mGluonNewIncomingChangesUpdater,
                        (Gluon.IncomingChangesNotificationPanel)mIncomingChangesNotificationPanel,
                        mParentWindow) as IIncomingChangesTab :
                    new Views.IncomingChanges.Developer.IncomingChangesTab(
                        mWkInfo,
                        mWorkspaceWindow,
                        this,
                        mDeveloperNewIncomingChangesUpdater,
                        mParentWindow);

                mViewHost.AddRefreshableView(
                    ViewType.IncomingChangesView,
                    (IRefreshableView)mIncomingChangesTab);

                PlasticAssetsProcessor.
                    RegisterIncomingChangesView(mIncomingChangesTab);
            }

            bool wasIncomingChangesSelected =
                IsViewSelected(SelectedTab.IncomingChanges);

            if (!wasIncomingChangesSelected)
                mIncomingChangesTab.AutoRefresh();

            SetSelectedView(SelectedTab.IncomingChanges);
        }

        void ShowChangesetsView()
        {
            if (mChangesetsTab == null)
            {
                mChangesetsTab = new ChangesetsTab(
                    mWkInfo,
                    mWorkspaceWindow,
                    this,
                    this,
                    mParentWindow,
                    mIsGluonMode);

                mViewHost.AddRefreshableView(
                    ViewType.ChangesetsView,
                    mChangesetsTab);
            }

            bool wasChangesetsSelected =
                IsViewSelected(SelectedTab.Changesets);

            if (!wasChangesetsSelected)
                ((IRefreshableView)mChangesetsTab).Refresh();

            SetSelectedView(SelectedTab.Changesets);
        }

        void ShowHistoryView(
            RepositorySpec repSpec,
            long itemId,
            string path,
            bool isDirectory)
        {
            if (mHistoryTab == null)
            {
                mHistoryTab = new HistoryTab(
                    mWkInfo,
                    mWorkspaceWindow,
                    repSpec,
                    mDeveloperNewIncomingChangesUpdater,
                    mViewHost,
                    mParentWindow,
                    mIsGluonMode);

                mViewHost.AddRefreshableView(
                    ViewType.HistoryView, mHistoryTab);
            }

            mHistoryTab.RefreshForItem(
                itemId,
                path,
                isDirectory);

            SetSelectedView(SelectedTab.History);
        }

        void InitializeTabButtonWidth()
        {
            if (mTabButtonWidth != -1)
                return;

            mTabButtonWidth = MeasureMaxWidth.ForTexts(
                UnityStyles.PlasticWindow.ActiveTabButton,
                PlasticLocalization.GetString(PlasticLocalization.Name.PendingChangesViewTitle),
                PlasticLocalization.GetString(PlasticLocalization.Name.IncomingChangesViewTitle),
                PlasticLocalization.GetString(PlasticLocalization.Name.ChangesetsViewTitle));
        }

        void ShowView(SelectedTab viewToShow)
        {
            switch (viewToShow)
            {
                case SelectedTab.PendingChanges:
                    ShowPendingChangesView();
                    break;

                case SelectedTab.IncomingChanges:
                    ShowIncomingChangesView();
                    break;

                case SelectedTab.Changesets:
                    ShowChangesetsView();
                    break;
            }
        }

        IRefreshableView GetRefreshableView(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.PendingChangesView:
                    return mPendingChangesTab;

                case ViewType.IncomingChangesView:
                    return (IRefreshableView)mIncomingChangesTab;

                case ViewType.ChangesetsView:
                    return mChangesetsTab;

                case ViewType.HistoryView:
                    return mHistoryTab;

                default:
                    return null;
            }
        }

        bool IsViewSelected(SelectedTab tab)
        {
            return mSelectedTab == tab;
        }

        void SetSelectedView(SelectedTab tab)
        {
            if (mSelectedTab != tab)
                mPreviousSelectedTab = mSelectedTab;

            mSelectedTab = tab;

            if (mIncomingChangesTab == null)
                return;

            mIncomingChangesTab.IsVisible =
                tab == SelectedTab.IncomingChanges;
        }

        void PendingChangesTabButtonGUI()
        {
            bool wasPendingChangesSelected =
                IsViewSelected(SelectedTab.PendingChanges);

            bool isPendingChangesSelected = mPendingChangesTabButton.
                DrawTabButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.PendingChangesViewTitle),
                    wasPendingChangesSelected,
                    mTabButtonWidth);

            if (isPendingChangesSelected)
                ShowPendingChangesView();
        }

        void IncomingChangesTabButtonGUI()
        {
            bool wasIncomingChangesSelected =
                IsViewSelected(SelectedTab.IncomingChanges);

            bool isIncomingChangesSelected = mIncomingChangesTabButton.
                DrawTabButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.IncomingChangesViewTitle),
                    wasIncomingChangesSelected,
                    mTabButtonWidth);

            if (isIncomingChangesSelected)
                ShowIncomingChangesView();
        }

        void ChangesetsTabButtonGUI()
        {
            bool wasChangesetsSelected =
                IsViewSelected(SelectedTab.Changesets);

            bool isChangesetsSelected = mChangesetsTabButton.
                DrawTabButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.ChangesetsViewTitle),
                    wasChangesetsSelected,
                    mTabButtonWidth);

            if (isChangesetsSelected)
                ShowChangesetsView();
        }

        void HistoryTabButtonGUI()
        {
            if (mHistoryTab == null)
                return;

            bool wasHistorySelected =
                IsViewSelected(SelectedTab.History);

            bool isCloseButtonClicked;

            bool isHistorySelected = mHistoryTabButton.
                DrawClosableTabButton(
                    PlasticLocalization.GetString(PlasticLocalization.Name.FileHistory),
                    wasHistorySelected,
                    true,
                    mTabButtonWidth,
                    mParentWindow.Repaint,
                    out isCloseButtonClicked);

            if (isCloseButtonClicked)
            {
                CloseHistoryTab();
                return;
            }

            if (isHistorySelected)
                SetSelectedView(SelectedTab.History);
        }

        enum SelectedTab
        {
            None = 0,
            PendingChanges = 1,
            IncomingChanges = 2,
            Changesets = 3,
            History = 4
        }

        PendingChangesTab mPendingChangesTab;
        IIncomingChangesTab mIncomingChangesTab;
        ChangesetsTab mChangesetsTab;
        HistoryTab mHistoryTab;

        SelectedTab mSelectedTab;
        SelectedTab mPreviousSelectedTab;

        float mTabButtonWidth = -1;

        TabButton mPendingChangesTabButton;
        TabButton mChangesetsTabButton;
        TabButton mIncomingChangesTabButton;
        TabButton mHistoryTabButton;

        WorkspaceWindow mWorkspaceWindow;

        readonly EditorWindow mParentWindow;
        readonly IAssetStatusCache mAssetStatusCache;
        readonly IIncomingChangesNotificationPanel mIncomingChangesNotificationPanel;
        readonly GluonNewIncomingChangesUpdater mGluonNewIncomingChangesUpdater;
        readonly NewIncomingChangesUpdater mDeveloperNewIncomingChangesUpdater;
        readonly PlasticGui.WorkspaceWindow.PendingChanges.PendingChanges mPendingChanges;
        readonly bool mIsGluonMode;
        readonly ViewHost mViewHost;
        readonly WorkspaceInfo mWkInfo;
    }
}