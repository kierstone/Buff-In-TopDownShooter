using UnityEditor;
using UnityEngine;

using Codice.Client.Commands;
using Codice.Client.Common.FsNodeReaders;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using Codice.Utils;
using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges.Dialogs
{
    internal class PendingChangesOptionsDialog : PlasticDialog
    {
        internal interface IAutorefreshView
        {
            void DisableAutorefresh();
            void EnableAutorefresh();
        }

        protected override Rect DefaultRect
        {
            get
            {
                var baseRect = base.DefaultRect;
                return new Rect(baseRect.x, baseRect.y, 667, 660);
            }
        }

        internal static void ChangeOptions(
            WorkspaceInfo wkInfo,
            IRefreshableView view,
            IAutorefreshView autorefreshView,
            EditorWindow window)
        {
            PendingChangesOptionsDialog dialog = Build(wkInfo);

            autorefreshView.DisableAutorefresh();

            bool isDialogDirty = false;

            try
            {
                if (dialog.RunModal(window) != ResponseType.Ok)
                    return;

                PendingChangesOptions currentOptions = dialog.GetOptions();
                isDialogDirty = dialog.IsDirty(currentOptions);

                if (!isDialogDirty)
                    return;

                currentOptions.SavePreferences();
            }
            finally
            {
                autorefreshView.EnableAutorefresh();

                if (isDialogDirty)
                    view.Refresh();
            }
        }

        protected override void OnModalGUI()
        {
            DoToolbarArea();

            DoOptionsArea();

            DoButtonsArea();
        }

        protected override string GetTitle()
        {
            return PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesOptionsTitle);
        }

        void DoToolbarArea()
        {
            var toolbarRect = GUILayoutUtility.GetRect(0, position.width, BOX_TOOLBAR_HEIGHT, BOX_TOOLBAR_HEIGHT);
            GUI.Box(new Rect(toolbarRect.x, toolbarRect.y + toolbarRect.height / 2, toolbarRect.width, BOX_HEIGHT), GUIContent.none);

            EditorGUI.BeginChangeCheck();

            mSelectedTab = (Tab)GUI.Toolbar(toolbarRect, (int)mSelectedTab, new string[]{
                PlasticLocalization.GetString(PlasticLocalization.Name.PendingChangesWhatToFindTab),
                PlasticLocalization.GetString(PlasticLocalization.Name.PendingChangesWhatToShowTab),
                PlasticLocalization.GetString(PlasticLocalization.Name.PendingChangesMoveDetectionTab)
            });

            if (EditorGUI.EndChangeCheck())
                EditorGUIUtility.keyboardControl = -1;
        }

        void DoOptionsArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(BOX_PADDING);

                using (new EditorGUILayout.VerticalScope(GUILayout.Height(BOX_HEIGHT)))
                {
                    GUILayout.Space(BOX_PADDING);

                    switch (mSelectedTab)
                    {
                        case Tab.WhatToFind:
                            DoWhatToFindTab();
                            break;
                        case Tab.WhatToShow:
                            DoWhatToShowTab();
                            break;
                        case Tab.MoveDetection:
                            DoMoveDetectionTab();
                            break;
                    }

                    GUILayout.Space(BOX_PADDING);
                }

                GUILayout.Space(BOX_PADDING);
            }
        }

        void DoWhatToFindTab()
        {
            mCanShowCheckouts = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesShowCheckouts), mCanShowCheckouts);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesShowCheckoutsExplanation));
            }

            GUILayout.Space(5);

            mCanFindChanged = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesFindChanged), mCanFindChanged);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFindChangedExplanation));
            }

            GUILayout.Space(5);

            mCanCheckFileContent = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesCheckFileContent), mCanCheckFileContent);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesCheckFileContentExplanation));
            }

            GUILayout.Space(60);
            DoFsWatcherMessage(mIsFileSystemWatcherEnabled);
        }

        void DoWhatToShowTab()
        {
            mCanAutoRefresh = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesAutoRefresh), mCanAutoRefresh);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesAutoRefreshExplanation));
            }

            GUILayout.Space(5);

            mCanShowPrivateFields = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesShowPrivateFiles), mCanShowPrivateFields);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesShowPrivateFilesExplanation));
            }

            GUILayout.Space(5);

            mCanShowIgnoredFiles = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesShowIgnoredFiles), mCanShowIgnoredFiles);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesShowIgnoredFilesExplanation));
            }

            GUILayout.Space(5);

            mCanShowHiddenFiles = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesShowHiddenFiles), mCanShowHiddenFiles);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesShowHiddenFilesExplanation));
            }

            GUILayout.Space(5);

            mCanShowDeletedFiles = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesShowDeletedFiles), mCanShowDeletedFiles);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesShowDeletedFilesExplanation));
            }
        }

        void DoMoveDetectionTab()
        {
            Paragraph(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesMoveDetectionExplanation));

            mCanFindMovedFiles = TitleToggle(PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesFindMovedFiles), mCanFindMovedFiles);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFindMovedFilesExplanation));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesMoveDetectionFineTune));
            }

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);

                GUI.enabled = mCanFindMovedFiles;

                mCanMatchBinarySameExtension = TitleToggle(
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesMatchBinarySameExtension),
                    mCanMatchBinarySameExtension);

                GUI.enabled = true;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(40);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesMatchBinarySameExtensionExplanation));
            }

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);

                GUI.enabled = mCanFindMovedFiles;

                mCanMatchTextSameExtension = TitleToggle(
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.PendingChangesMatchTextSameExtension),
                    mCanMatchTextSameExtension);

                GUI.enabled = true;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(40);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesMatchTextSameExtensionExplanation));
            }

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                GUILayout.Label(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesSimilarityPercentage),
                    UnityStyles.Dialog.Toggle);
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(8);
                    mSimilarityPercent = Mathf.RoundToInt(GUILayout.HorizontalSlider(
                        mSimilarityPercent, 40, 100, GUILayout.Width(300)));

                }
                GUILayout.Space(2);
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(8);
                    GUILayout.Label(mSimilarityPercent + "%");
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(40);
                Paragraph(PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesSimilarityPercentageExplanation));
            }
        }

        void DoFsWatcherMessage(bool isEnabled)
        {
            GUIContent message = new GUIContent(
                isEnabled ?
                    GetFsWatcherEnabledMessage() :
                    GetFsWatcherDisabledMessage(),
                isEnabled ?
                    Images.GetInfoIcon() :
                    Images.GetWarnIcon());

            GUILayout.Label(message, UnityStyles.Dialog.Toggle, GUILayout.Height(32));
            GUILayout.Space(-10);

            string formattedExplanation = isEnabled ?
                GetFsWatcherEnabledExplanation() :
                GetFsWatcherDisabledExplanation();

            string helpLink = GetHelpLink();

            TextBlockWithEndLink(
                helpLink, formattedExplanation, UnityStyles.Paragraph);
        }

        void DoButtonsArea()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    DoOkButton();
                    DoCancelButton();
                    return;
                }

                DoCancelButton();
                DoOkButton();
            }
        }

        void DoOkButton()
        {
            if (!AcceptButton(PlasticLocalization.GetString(
                    PlasticLocalization.Name.OkButton)))
                return;

            OkButtonAction();
        }

        void DoCancelButton()
        {
            if (!NormalButton(PlasticLocalization.GetString(
                    PlasticLocalization.Name.CancelButton)))
                return;

            CancelButtonAction();
        }

        bool IsDirty(PendingChangesOptions currentOptions)
        {
            return !mInitialOptions.AreSameOptions(currentOptions);
        }

        PendingChangesOptions GetOptions()
        {
            WorkspaceStatusOptions resultWkStatusOptions =
                WorkspaceStatusOptions.None;

            if (mCanShowCheckouts)
            {
                resultWkStatusOptions |= WorkspaceStatusOptions.FindCheckouts;
                resultWkStatusOptions |= WorkspaceStatusOptions.FindReplaced;
                resultWkStatusOptions |= WorkspaceStatusOptions.FindCopied;
            }

            if (mCanFindChanged)
                resultWkStatusOptions |= WorkspaceStatusOptions.FindChanged;
            if (mCanShowPrivateFields)
                resultWkStatusOptions |= WorkspaceStatusOptions.FindPrivates;
            if (mCanShowDeletedFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.FindLocallyDeleted;
            if (mCanShowIgnoredFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.ShowIgnored;
            if (mCanShowHiddenFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.ShowHiddenChanges;
            if (mCanFindMovedFiles)
                resultWkStatusOptions |= WorkspaceStatusOptions.CalculateLocalMoves;

            MovedMatchingOptions matchingOptions = new MovedMatchingOptions();
            matchingOptions.AllowedChangesPerUnit =
                (100 - mSimilarityPercent) / 100f;
            matchingOptions.bBinMatchingOnlySameExtension =
                mCanMatchBinarySameExtension;
            matchingOptions.bTxtMatchingOnlySameExtension =
                mCanMatchTextSameExtension;

            return new PendingChangesOptions(
                resultWkStatusOptions, matchingOptions,
                mCanAutoRefresh, false,
                mCanCheckFileContent, false);
        }

        void SetOptions(PendingChangesOptions options)
        {
            mCanShowCheckouts = IsEnabled(
                WorkspaceStatusOptions.FindCheckouts, options.WorkspaceStatusOptions);
            mCanFindChanged = IsEnabled(
                WorkspaceStatusOptions.FindChanged, options.WorkspaceStatusOptions);
            mCanCheckFileContent = options.CheckFileContentForChanged;

            mCanAutoRefresh = options.AutoRefresh;
            mCanShowPrivateFields = IsEnabled(
                WorkspaceStatusOptions.FindPrivates, options.WorkspaceStatusOptions);
            mCanShowIgnoredFiles = IsEnabled(
                WorkspaceStatusOptions.ShowIgnored, options.WorkspaceStatusOptions);
            mCanShowHiddenFiles = IsEnabled(
                WorkspaceStatusOptions.ShowHiddenChanges, options.WorkspaceStatusOptions);
            mCanShowDeletedFiles = IsEnabled(
                WorkspaceStatusOptions.FindLocallyDeleted, options.WorkspaceStatusOptions);

            mCanFindMovedFiles = IsEnabled(
                WorkspaceStatusOptions.CalculateLocalMoves, options.WorkspaceStatusOptions);
            mCanMatchBinarySameExtension =
                options.MovedMatchingOptions.bBinMatchingOnlySameExtension;
            mCanMatchTextSameExtension =
                options.MovedMatchingOptions.bTxtMatchingOnlySameExtension;
            mSimilarityPercent = (int)((1 - options.MovedMatchingOptions.AllowedChangesPerUnit) * 100f);
        }

        void CheckFsWatcher(WorkspaceInfo wkInfo)
        {
            bool isFileSystemWatcherEnabled = false;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                                            {
                                                isFileSystemWatcherEnabled =
                                                    IsFileSystemWatcherEnabled(wkInfo);
                                            },
                /*afterOperationDelegate*/ delegate
                                           {
                                               if (waiter.Exception != null)
                                                   return;

                                               mIsFileSystemWatcherEnabled = isFileSystemWatcherEnabled;
                                           });
        }

        static bool IsEnabled(WorkspaceStatusOptions option, WorkspaceStatusOptions options)
        {
            return (options & option) == option;
        }

        static bool IsFileSystemWatcherEnabled(WorkspaceInfo wkInfo)
        {
            return WorkspaceWatcherFsNodeReadersCache.Get().
                IsWatcherEnabled(wkInfo);
        }

        static string GetFsWatcherEnabledMessage()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFilesystemWatcherEnabled);

            return PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesINotifyEnabled);
        }

        static string GetFsWatcherDisabledMessage()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFilesystemWatcherDisabled);

            return PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesINotifyDisabled);
        }

        static string GetFsWatcherEnabledExplanation()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFilesystemWatcherEnabledExplanation);

            return PlasticLocalization.GetString(
            PlasticLocalization.Name.PendingChangesINotifyEnabledExplanation);
        }

        static string GetFsWatcherDisabledExplanation()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
            {
                return PlasticLocalization.GetString(
                    PlasticLocalization.Name.PendingChangesFilesystemWatcherDisabledExplanation)
                    .Replace("[[HELP_URL|{0}]]", "{0}");
            }

            return PlasticLocalization.GetString(
                PlasticLocalization.Name.PendingChangesINotifyDisabledExplanation);
        }

        static string GetHelpLink()
        {
            if (PlatformIdentifier.IsWindows() || PlatformIdentifier.IsMac())
                return FS_WATCHER_HELP_URL;

            return INOTIFY_HELP_URL;
        }

        static PendingChangesOptionsDialog Build(WorkspaceInfo wkInfo)
        {
            PendingChangesOptionsDialog result = Create();

            result.CheckFsWatcher(wkInfo);

            result.mInitialOptions = new PendingChangesOptions();
            result.mInitialOptions.LoadPendingChangesOptions();
            result.SetOptions(result.mInitialOptions);

            return result;
        }

        static PendingChangesOptionsDialog Create()
        {
            var instance = CreateInstance<PendingChangesOptionsDialog>();
            instance.mEnterKeyAction = instance.OkButtonAction;
            instance.mEscapeKeyAction = instance.CancelButtonAction;
            return instance;
        }

        enum Tab
        {
            WhatToFind,
            WhatToShow,
            MoveDetection
        }

        Tab mSelectedTab = Tab.WhatToFind;

        bool mCanShowCheckouts;
        bool mCanAutoRefresh;
        bool mCanFindChanged;
        bool mCanCheckFileContent;
        bool mCanShowPrivateFields;
        bool mIsFileSystemWatcherEnabled;
        bool mCanShowIgnoredFiles;
        bool mCanShowHiddenFiles;
        bool mCanShowDeletedFiles;
        bool mCanFindMovedFiles;
        bool mCanMatchBinarySameExtension;
        bool mCanMatchTextSameExtension;
        int mSimilarityPercent;

        PendingChangesOptions mInitialOptions;

        const float BOX_PADDING = 22f;
        const float BOX_TOOLBAR_HEIGHT = 20f;
        const float BOX_HEIGHT = 560f;

        const string FS_WATCHER_HELP_URL = "https://plasticscm.com/download/help/support";
        const string INOTIFY_HELP_URL = "https://plasticscm.com/download/help/inotify";
    }
}
