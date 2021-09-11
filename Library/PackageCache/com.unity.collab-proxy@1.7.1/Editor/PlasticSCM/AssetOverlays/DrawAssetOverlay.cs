using System;

using UnityEditor;
using UnityEngine;

using PlasticGui;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.AssetsOverlays
{
    internal static class DrawAssetOverlay
    {
        internal static void Initialize(
            IAssetStatusCache cache,
            Action repaintProjectWindow)
        {
            mAssetStatusCache = cache;
            mRepaintProjectWindow = repaintProjectWindow;

            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

            mRepaintProjectWindow();
        }

        internal static void Dispose()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;

            if (mRepaintProjectWindow != null)
                mRepaintProjectWindow();
        }

        internal static void ClearCache()
        {
            mAssetStatusCache.Clear();
            mRepaintProjectWindow();
        }

        internal static AssetStatus GetStatusesToDraw(AssetStatus status)
        {
            if (status.HasFlag(AssetStatus.Checkout) &&
                status.HasFlag(AssetStatus.Locked))
                return status & ~AssetStatus.Checkout;

            if (status.HasFlag(AssetStatus.DeletedOnServer) &&
                status.HasFlag(AssetStatus.LockedRemote))
                return status & ~AssetStatus.LockedRemote;

            return status;
        }

        internal static string GetStatusString(AssetStatus statusValue)
        {
            switch (statusValue)
            {
                case AssetStatus.Private:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.Private);
                case AssetStatus.Ignored:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.StatusIgnored);
                case AssetStatus.Added:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.StatusAdded);
                case AssetStatus.Checkout:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.StatusCheckout);
                case AssetStatus.OutOfDate:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.StatusOutOfDate);
                case AssetStatus.Conflicted:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.StatusConflicted);
                case AssetStatus.DeletedOnServer:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.StatusDeletedOnServer);
                case AssetStatus.Locked:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.StatusLockedMe);
                case AssetStatus.LockedRemote:
                    return PlasticLocalization.GetString(
                        PlasticLocalization.Name.StatusLockedRemote);
            }

            return string.Empty;
        }

        internal static string GetTooltipText(
            AssetStatus statusValue,
            LockStatusData lockStatusData)
        {
            string statusText = GetStatusString(statusValue);

            if (lockStatusData == null)
                return statusText;

            // example:
            // Changed by:
            // * dani_pen@hotmail.com
            // * workspace wkLocal"

            char bulletCharacter = '\u25cf';

            string line1 = PlasticLocalization.GetString(
                PlasticLocalization.Name.AssetOverlayTooltipStatus, statusText);

            string line2 = string.Format("{0} {1}",
                bulletCharacter,
                lockStatusData.LockedBy);

            string line3 = string.Format("{0} {1}",
                bulletCharacter,
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.AssetOverlayTooltipWorkspace,
                    lockStatusData.WorkspaceName));

            return string.Format(
                "{0}" + Environment.NewLine +
                "{1}" + Environment.NewLine +
                "{2}",
                line1,
                line2,
                line3);
        }

        static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            if (Event.current.type != EventType.Repaint)
                return;

            AssetStatus statusesToDraw = GetStatusesToDraw(
                mAssetStatusCache.GetStatusForGuid(guid));

            foreach (AssetStatus status in Enum.GetValues(typeof(AssetStatus)))
            {
                if (status == AssetStatus.None)
                    continue;

                if (!statusesToDraw.HasFlag(status))
                    continue;

                LockStatusData lockStatusData =
                    ClassifyAssetStatus.IsLockedRemote(status) ?
                        mAssetStatusCache.GetLockStatusData(guid) :
                        null;

                string tooltipText = GetTooltipText(
                    status,
                    lockStatusData);

                DrawOverlayIcon.ForStatus(
                    selectionRect,
                    status,
                    tooltipText);
            }
        }

        internal static class DrawOverlayIcon
        {
            internal static void ForStatus(
                Rect selectionRect,
                AssetStatus status,
                string tooltipText)
            {
                Texture overlayIcon = GetOverlayIcon(status);

                if (overlayIcon == null)
                    return;

                Rect overlayRect = GetOverlayRect(
                    selectionRect, overlayIcon, status);

                GUI.DrawTexture(
                    overlayRect, overlayIcon, ScaleMode.ScaleToFit);

                Rect tooltipRect = GetTooltipRect(selectionRect, overlayRect);

                GUI.Label(tooltipRect, new GUIContent(string.Empty, tooltipText));
            }

            internal static Texture GetOverlayIcon(AssetStatus status)
            {
                switch (status)
                {
                    case AssetStatus.Ignored:
                        return Images.GetImage(Images.Name.Ignored);
                    case AssetStatus.Private:
                        return Images.GetPrivatedOverlayIcon();
                    case AssetStatus.Added:
                        return Images.GetAddedOverlayIcon();
                    case AssetStatus.Checkout:
                        return Images.GetCheckedOutOverlayIcon();
                    case AssetStatus.OutOfDate:
                        return Images.GetOutOfSyncOverlayIcon();
                    case AssetStatus.Conflicted:
                        return Images.GetConflictedOverlayIcon();
                    case AssetStatus.DeletedOnServer:
                        return Images.GetDeletedRemoteOverlayIcon();
                    case AssetStatus.Locked:
                        return Images.GetLockedLocalOverlayIcon();
                    case AssetStatus.LockedRemote:
                        return Images.GetLockedRemoteOverlayIcon();
                }

                return null;
            }

            static Rect Inflate(Rect rect, float width, float height)
            {
                return new Rect(
                    rect.x - width,
                    rect.y - height,
                    rect.width + 2 * width,
                    rect.height + 2 * height);
            }

            static Rect GetOverlayRect(
                Rect selectionRect,
                Texture overlayIcon,
                AssetStatus status)
            {
                OverlayAlignment alignment = GetIconPosition(status);

                if (selectionRect.width > selectionRect.height)
                    return GetOverlayRectForSmallestSize(
                        selectionRect, overlayIcon, alignment);

                return GetOverlayRectForOtherSizes(
                    selectionRect, overlayIcon, alignment);
            }

            static Rect GetTooltipRect(
                Rect selectionRect,
                Rect overlayRect)
            {
                if (selectionRect.width > selectionRect.height)
                {
                    return overlayRect;
                }

                return Inflate(overlayRect, 3, 3);
            }

            static Rect GetOverlayRectForSmallestSize(
                Rect selectionRect,
                Texture overlayIcon,
                OverlayAlignment alignment)
            {
                float xOffset = IsLeftAligned(alignment) ? -5 : 5;
                float yOffset = IsTopAligned(alignment) ? -4 : 4;

                return new Rect(
                    selectionRect.x + xOffset,
                    selectionRect.y + yOffset,
                    OVERLAY_ICON_SIZE,
                    OVERLAY_ICON_SIZE);
            }

            static Rect GetOverlayRectForOtherSizes(
                Rect selectionRect,
                Texture overlayIcon,
                OverlayAlignment alignment)
            {
                float xOffset = IsLeftAligned(alignment) ?
                    0 : selectionRect.width - overlayIcon.width;

                float yOffset = IsTopAligned(alignment) ?
                    0 : selectionRect.height - overlayIcon.height - 12;

                return new Rect(
                    selectionRect.x + xOffset,
                    selectionRect.y + yOffset,
                    OVERLAY_ICON_SIZE,
                    OVERLAY_ICON_SIZE);
            }

            static OverlayAlignment GetIconPosition(AssetStatus status)
            {
                if (status == AssetStatus.Checkout ||
                    status == AssetStatus.Locked)
                    return OverlayAlignment.TopLeft;

                if (status == AssetStatus.DeletedOnServer ||
                    status == AssetStatus.LockedRemote)
                    return OverlayAlignment.TopRight;

                if (status == AssetStatus.OutOfDate)
                    return OverlayAlignment.BottomRight;

                return OverlayAlignment.BottomLeft;
            }

            static bool IsLeftAligned(OverlayAlignment alignment)
            {
                return alignment == OverlayAlignment.BottomLeft ||
                       alignment == OverlayAlignment.TopLeft;
            }

            static bool IsTopAligned(OverlayAlignment alignment)
            {
                return alignment == OverlayAlignment.TopLeft ||
                       alignment == OverlayAlignment.TopRight;
            }

            enum OverlayAlignment
            {
                TopLeft,
                BottomLeft,
                TopRight,
                BottomRight
            }
        }

        static IAssetStatusCache mAssetStatusCache;
        static Action mRepaintProjectWindow;

        const float OVERLAY_ICON_SIZE = 16;
    }
}

