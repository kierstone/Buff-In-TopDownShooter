using System.IO;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using Codice.LogWrapper;
using PlasticGui.Help;
using Unity.PlasticSCM.Editor.AssetUtils;

namespace Unity.PlasticSCM.Editor.UI
{
    internal class Images
    {
        internal enum Name
        {
            None,

            IconPlastic,
            IconCloseButton,
            IconPressedCloseButton,
            IconAdded,
            IconDeleted,
            IconChanged,
            IconMoved,
            IconMergeLink,
            Ignored,
            IconMergeConflict,
            IconMerged,
            IconFsChanged,
            IconMergeCategory,
            XLink,
            Ok,
            NotOnDisk,
            IconRepository,
            IconPlasticView,
            IconPlasticViewNotify,
            Loading,
            IconEmptyGravatar,
            Step1,
            Step2,
            Step3,
            StepOk,
            ButtonSsoSignInUnity,
            ButtonSsoSignInEmail,
            ButtonSsoSignInGoogle,
        }

        internal static Texture2D GetHelpImage(HelpImage image)
        {
            // We use the dark version for both the light/dark skins since it matches the grey background better
            string helpImageFileName = string.Format(
                "d_{0}.png",
                HelpImageName.FromHelpImage(image));

            string imageRelativePath = GetImageFileRelativePath(helpImageFileName);
            Texture2D result = TryLoadImage(imageRelativePath, imageRelativePath);

            if (result != null)
                return result;

            mLog.WarnFormat("Image not found: {0}", helpImageFileName);
            return GetEmptyImage();
        }

        internal static Texture2D GetImage(Name image)
        {
            string imageFileName = image.ToString().ToLower() + ".png";
            string imageFileName2x = image.ToString().ToLower() + "@2x.png";

            string darkImageFileName = string.Format("d_{0}", imageFileName);
            string darkImageFileName2x = string.Format("d_{0}", imageFileName2x);

            string imageFileRelativePath = GetImageFileRelativePath(imageFileName);
            string imageFileRelativePath2x = GetImageFileRelativePath(imageFileName);

            string darkImageFileRelativePath = GetImageFileRelativePath(darkImageFileName);
            string darkImageFileRelativePath2x = GetImageFileRelativePath(darkImageFileName2x);

            Texture2D result = null;

            if (EditorGUIUtility.isProSkin)
                result = TryLoadImage(darkImageFileRelativePath, darkImageFileRelativePath2x);

            if (result != null)
                return result;

            result = TryLoadImage(imageFileRelativePath, imageFileRelativePath2x);

            if (result != null)
                return result;

            mLog.WarnFormat("Image not found: {0}", imageFileName);
            return GetEmptyImage();
        }

        internal static Texture GetFileIcon(string path)
        {
            string relativePath = GetRelativePath.ToApplication(path);

            return GetFileIconFromRelativePath(relativePath);
        }

        internal static Texture GetFileIconFromCmPath(string path)
        {
            return GetFileIconFromRelativePath(
                path.Substring(1).Replace("/",
                Path.DirectorySeparatorChar.ToString()));
        }

        internal static Texture GetDropDownIcon()
        {
            if (mPopupIcon == null)
                mPopupIcon = EditorGUIUtility.IconContent("icon dropdown").image;

            return mPopupIcon;
        }

        internal static Texture GetDirectoryIcon()
        {
            if (mDirectoryIcon == null)
                mDirectoryIcon = EditorGUIUtility.IconContent("Folder Icon").image;

            return mDirectoryIcon;
        }

        internal static Texture GetPrivatedOverlayIcon()
        {
            if (mPrivatedOverlayIcon == null)
                mPrivatedOverlayIcon = EditorGUIUtility.IconContent("P4_Local").image;

            return mPrivatedOverlayIcon;
        }

        internal static Texture GetAddedOverlayIcon()
        {
            if (mAddedOverlayIcon == null)
                mAddedOverlayIcon = EditorGUIUtility.IconContent("P4_AddedLocal").image;

            return mAddedOverlayIcon;
        }

        internal static Texture GetDeletedOverlayIcon()
        {
            if (mDeletedOverlayIcon == null)
                mDeletedOverlayIcon = EditorGUIUtility.IconContent("P4_DeletedLocal").image;

            return mDeletedOverlayIcon;
        }

        internal static Texture GetCheckedOutOverlayIcon()
        {
            if (mCheckedOutOverlayIcon == null)
                mCheckedOutOverlayIcon = EditorGUIUtility.IconContent("P4_CheckOutLocal").image;

            return mCheckedOutOverlayIcon;
        }

        internal static Texture GetDeletedRemoteOverlayIcon()
        {
            if (mDeletedRemoteOverlayIcon == null)
                mDeletedRemoteOverlayIcon = EditorGUIUtility.IconContent("P4_DeletedRemote").image;

            return mDeletedRemoteOverlayIcon;
        }

        internal static Texture GetOutOfSyncOverlayIcon()
        {
            if (mOutOfSyncOverlayIcon == null)
                mOutOfSyncOverlayIcon = EditorGUIUtility.IconContent("P4_OutOfSync").image;

            return mOutOfSyncOverlayIcon;
        }

        internal static Texture GetConflictedOverlayIcon()
        {
            if (mConflictedOverlayIcon == null)
                mConflictedOverlayIcon = EditorGUIUtility.IconContent("P4_Conflicted").image;

            return mConflictedOverlayIcon;
        }

        internal static Texture GetLockedLocalOverlayIcon()
        {
            if (mLockedLocalOverlayIcon == null)
                mLockedLocalOverlayIcon = EditorGUIUtility.IconContent("P4_LockedLocal").image;

            return mLockedLocalOverlayIcon;
        }

        internal static Texture GetLockedRemoteOverlayIcon()
        {
            if (mLockedRemoteOverlayIcon == null)
                mLockedRemoteOverlayIcon = EditorGUIUtility.IconContent("P4_LockedRemote").image;

            return mLockedRemoteOverlayIcon;
        }

        internal static Texture GetWarnIcon()
        {
            if (mWarnIcon == null)
                mWarnIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;

            return mWarnIcon;
        }

        internal static Texture GetInfoIcon()
        {
            if (mInfoIcon == null)
                mInfoIcon = EditorGUIUtility.IconContent("console.infoicon.sml").image;

            return mInfoIcon;
        }

        internal static Texture GetErrorDialogIcon()
        {
            if (mErrorDialogIcon == null)
                mErrorDialogIcon = EditorGUIUtility.IconContent("console.erroricon").image;

            return mErrorDialogIcon;
        }

        internal static Texture GetWarnDialogIcon()
        {
            if (mWarnDialogIcon == null)
                mWarnDialogIcon = EditorGUIUtility.IconContent("console.warnicon").image;

            return mWarnDialogIcon;
        }

        internal static Texture GetInfoDialogIcon()
        {
            if (mInfoDialogIcon == null)
                mInfoDialogIcon = EditorGUIUtility.IconContent("console.infoicon").image;

            return mInfoDialogIcon;
        }

        internal static Texture GetRefreshIcon()
        {
            if (mRefreshIcon == null)
                mRefreshIcon = EditorGUIUtility.FindTexture("Refresh");

            return mRefreshIcon;
        }

        internal static Texture GetCloseIcon()
        {
            if (mCloseIcon == null)
                mCloseIcon = EditorGUIUtility.FindTexture("winbtn_win_close");

            return mCloseIcon;
        }

        internal static Texture GetClickedCloseIcon()
        {
            if (mClickedCloseIcon == null)
                mClickedCloseIcon = EditorGUIUtility.FindTexture("winbtn_win_close_a");

            return mClickedCloseIcon;
        }

        internal static Texture GetHoveredCloseIcon()
        {
            if (mHoveredCloseIcon == null)
                mHoveredCloseIcon = EditorGUIUtility.FindTexture("winbtn_win_close_h");

            return mHoveredCloseIcon;
        }

        internal static Texture GetFileIcon()
        {
            if (mFileIcon == null)
                mFileIcon = EditorGUIUtility.FindTexture("DefaultAsset Icon");

            if (mFileIcon == null)
                mFileIcon = AssetPreview.GetMiniTypeThumbnail(typeof(DefaultAsset));

            if (mFileIcon == null)
                mFileIcon = GetEmptyImage();

            return mFileIcon;
        }

        internal static Texture2D GetLinkUnderlineImage()
        {
            if (mLinkUnderlineImage == null)
            {
                mLinkUnderlineImage = new Texture2D(1, 1);
                mLinkUnderlineImage.SetPixel(0, 0, UnityStyles.Colors.Link);
                mLinkUnderlineImage.Apply();
            }

            return mLinkUnderlineImage;
        }

        static Texture2D GetEmptyImage()
        {
            if (mEmptyImage == null)
            {
                mEmptyImage = new Texture2D(1, 1);
                mEmptyImage.SetPixel(0, 0, Color.clear);
                mEmptyImage.Apply();
            }

            return mEmptyImage;
        }

        static Texture GetFileIconFromRelativePath(string relativePath)
        {
            Texture result = AssetDatabase.GetCachedIcon(relativePath);

            if (result == null)
                return GetFileIcon();

            return result;
        }

        static string GetImageFileRelativePath(string imageFileName)
        {
            return Path.Combine(
                AssetsPath.GetImagesFolderRelativePath(),
                imageFileName);
        }

        static Texture2D TryLoadImage(string imageFileRelativePath, string image2xFilePath)
        {
            if (EditorGUIUtility.pixelsPerPoint > 1f && File.Exists(image2xFilePath))
                return LoadTextureFromFile(image2xFilePath);

            if (File.Exists(Path.GetFullPath(imageFileRelativePath)))
                return LoadTextureFromFile(imageFileRelativePath);

            return null;
        }

        static Texture2D LoadTextureFromFile(string path)
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D result = new Texture2D(1, 1);
            result.LoadImage(fileData); //auto-resizes the texture dimensions
            return result;
        }

        static Texture mFileIcon;
        static Texture mDirectoryIcon;

        static Texture mPrivatedOverlayIcon;
        static Texture mAddedOverlayIcon;
        static Texture mDeletedOverlayIcon;
        static Texture mCheckedOutOverlayIcon;
        static Texture mDeletedRemoteOverlayIcon;
        static Texture mOutOfSyncOverlayIcon;
        static Texture mConflictedOverlayIcon;
        static Texture mLockedLocalOverlayIcon;
        static Texture mLockedRemoteOverlayIcon;

        static Texture mWarnIcon;
        static Texture mInfoIcon;

        static Texture mErrorDialogIcon;
        static Texture mWarnDialogIcon;
        static Texture mInfoDialogIcon;

        static Texture mRefreshIcon;

        static Texture mCloseIcon;
        static Texture mClickedCloseIcon;
        static Texture mHoveredCloseIcon;

        static Texture2D mLinkUnderlineImage;

        static Texture2D mEmptyImage;

        static Texture mPopupIcon;

        static readonly ILog mLog = LogManager.GetLogger("Images");
    }
}