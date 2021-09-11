using System.IO;

using Unity.PlasticSCM.Editor.AssetsOverlays;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;

namespace Unity.PlasticSCM.Editor.AssetUtils.Processor
{
    class AssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        internal static bool IsEnabled { get; set; }

        internal static void RegisterAssetStatusCache(
            IAssetStatusCache assetStatusCache)
        {
            mAssetStatusCache = assetStatusCache;
        }

        static string[] OnWillSaveAssets(string[] paths)
        {
            if (!IsEnabled)
                return paths;

            PlasticAssetsProcessor.CheckoutOnSourceControl(paths);
            return paths;
        }

        static bool IsOpenForEdit(string assetPath, out string message)
        {
            message = string.Empty;

            if (!IsEnabled)
                return true;

            if (assetPath.StartsWith("ProjectSettings/"))
                return true;

            if (MetaPath.IsMetaPath(assetPath))
                assetPath = MetaPath.GetPathFromMetaPath(assetPath);

            AssetStatus status = mAssetStatusCache.GetStatusForPath(
                Path.GetFullPath(assetPath));

            if (ClassifyAssetStatus.IsAdded(status) ||
                ClassifyAssetStatus.IsCheckedOut(status))
                return true;

            return !ClassifyAssetStatus.IsControlled(status);
        }

        static IAssetStatusCache mAssetStatusCache;
    }
}
