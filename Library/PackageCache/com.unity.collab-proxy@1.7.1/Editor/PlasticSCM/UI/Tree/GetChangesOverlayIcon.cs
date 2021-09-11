using UnityEngine;

using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Codice.ThemeImages;
using PlasticGui.WorkspaceWindow.IncomingChanges;
using PlasticGui.WorkspaceWindow.PendingChanges;

using GluonIncomingChangeInfo = PlasticGui.Gluon.WorkspaceWindow.Views.IncomingChanges.IncomingChangeInfo;
using GluonIncomingChangeCategory = PlasticGui.Gluon.WorkspaceWindow.Views.IncomingChanges.IncomingChangeCategory;
using Unity.PlasticSCM.Editor.AssetsOverlays;

namespace Unity.PlasticSCM.Editor.UI.Tree
{
    static class GetChangesOverlayIcon
    {
        internal class Data
        {
            internal readonly Texture Texture;
            internal readonly float XOffset;
            internal readonly float YOffset;
            internal readonly float Size;

            internal Data(Texture texture, float xOffset, float yOffset, float size)
            {
                Texture = texture;
                XOffset = xOffset;
                YOffset = yOffset;
                Size = size;
            }
        }

        internal static Data ForPlasticIncomingChange(
            IncomingChangeInfo incomingChange,
            bool isSolvedConflict)
        {
            if (incomingChange.CategoryType == IncomingChangesCategory.Type.FileConflicts ||
                incomingChange.CategoryType == IncomingChangesCategory.Type.DirectoryConflicts)
                return ForConflict(isSolvedConflict);

            if (incomingChange.IsXLink())
                return BuildData.ForXLink();

            if (incomingChange.CategoryType == IncomingChangesCategory.Type.Deleted)
                return BuildData.ForDeletedOnServer();

            if (incomingChange.CategoryType == IncomingChangesCategory.Type.Changed)
                return BuildData.ForOutOfDate();

            return null;
        }

        internal static Data ForGluonIncomingChange(
            GluonIncomingChangeInfo incomingChange,
            bool isSolvedConflict)
        {
            if (incomingChange.CategoryType == GluonIncomingChangeCategory.Type.Conflicted)
                return ForConflict(isSolvedConflict);

            if (incomingChange.IsXLink())
                return BuildData.ForXLink();

            if (incomingChange.CategoryType == GluonIncomingChangeCategory.Type.Deleted)
                return BuildData.ForDeletedOnServer();

            if (incomingChange.CategoryType == GluonIncomingChangeCategory.Type.Changed)
                return BuildData.ForOutOfDate();

            return null;
        }

        internal static Data ForPendingChange(
            ChangeInfo changeInfo,
            bool isConflict)
        {
            if (isConflict)
                return BuildData.ForConflicted();

            ItemIconImageType type = ChangeInfoView.
                GetIconImageType(changeInfo);

            if (ChangeTypesOperator.AreAllSet(
                    changeInfo.ChangeTypes, ChangeTypes.Added))
                return BuildData.ForAdded();

            switch (type)
            {
                case ItemIconImageType.Ignored:
                    return BuildData.ForIgnored();
                case ItemIconImageType.Private:
                    return BuildData.ForPrivated();
                case ItemIconImageType.Deleted:
                    return BuildData.ForDeleted();
                case ItemIconImageType.CheckedOut:
                    return BuildData.ForCheckedOut();
                default:
                    return null;
            }
        }

        internal static Data ForAssetStatus(AssetStatus status)
        {
            switch (status)
            {
                case AssetStatus.Ignored:
                    return BuildData.ForIgnored();
                case AssetStatus.Private:
                    return BuildData.ForPrivated();
                case AssetStatus.Added:
                    return BuildData.ForAdded();
                case AssetStatus.Checkout:
                    return BuildData.ForCheckedOut();
                case AssetStatus.OutOfDate:
                    return BuildData.ForOutOfDate();
                case AssetStatus.Conflicted:
                    return BuildData.ForConflicted();
                case AssetStatus.DeletedOnServer:
                    return BuildData.ForDeletedOnServer();
                case AssetStatus.Locked:
                    return BuildData.ForLocked();
                case AssetStatus.LockedRemote:
                    return BuildData.ForLockedRemote();
            }

            return null;
        }

        static Data ForConflict(bool isResolved)
        {
            if (isResolved)
                return BuildData.ForOk();

            return BuildData.ForConflicted();
        }

        static class BuildData
        {
            internal static Data ForOk()
            {
                return new Data(
                    Images.GetImage(Images.Name.Ok),
                    4f, 4f, SIZE);
            }

            internal static Data ForXLink()
            {
                return new Data(
                    Images.GetImage(Images.Name.XLink),
                    2f, 3f, SIZE);
            }

            internal static Data ForIgnored()
            {
                return new Data(
                    Images.GetImage(Images.Name.Ignored),
                    GetLeftXOffset(),
                    GetBottomYOffset(),
                    SIZE);
            }

            internal static Data ForPrivated()
            {
                return new Data(
                    Images.GetPrivatedOverlayIcon(),
                    GetLeftXOffset(),
                    GetBottomYOffset(),
                    SIZE);
            }

            internal static Data ForAdded()
            {
                return new Data(
                    Images.GetAddedOverlayIcon(),
                    GetLeftXOffset(),
                    GetTopYOffset(),
                    SIZE);
            }

            internal static Data ForDeleted()
            {
                return new Data(
                    Images.GetDeletedOverlayIcon(),
                    GetLeftXOffset(),
                    GetTopYOffset(),
                    SIZE);
            }

            internal static Data ForCheckedOut()
            {
                return new Data(
                    Images.GetCheckedOutOverlayIcon(),
                    GetLeftXOffset(),
                    GetTopYOffset(),
                    SIZE);
            }

            internal static Data ForDeletedOnServer()
            {
                return new Data(
                    Images.GetDeletedRemoteOverlayIcon(),
                    GetRightXOffset(),
                    GetTopYOffset(),
                    SIZE);
            }

            internal static Data ForOutOfDate()
            {
                return new Data(
                    Images.GetOutOfSyncOverlayIcon(),
                    GetRightXOffset(),
                    GetBottomYOffset(),
                    SIZE);
            }

            internal static Data ForLocked()
            {
                return new Data(
                    Images.GetLockedLocalOverlayIcon(),
                    GetLeftXOffset(),
                    GetTopYOffset(),
                    SIZE);
            }

            internal static Data ForLockedRemote()
            {
                return new Data(
                    Images.GetLockedRemoteOverlayIcon(),
                    GetRightXOffset(),
                    GetTopYOffset(),
                    SIZE);
            }

            static float GetLeftXOffset()
            {
                return -4f;
            }

            internal static Data ForConflicted()
            {
                return new Data(
                    Images.GetConflictedOverlayIcon(),
                    GetLeftXOffset(),
                    GetBottomYOffset(),
                    SIZE);
            }

            static float GetRightXOffset()
            {
                return 8f;
            }

            static float GetBottomYOffset()
            {
                return UnityConstants.TREEVIEW_ROW_HEIGHT - SIZE + 2f;
            }

            static float GetTopYOffset()
            {
                return -1f;
            }

            const float SIZE = 16;
        }
    }
}
