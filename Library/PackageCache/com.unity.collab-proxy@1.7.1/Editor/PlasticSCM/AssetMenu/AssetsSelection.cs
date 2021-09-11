using System;
using System.Collections.Generic;
using System.IO;
using Unity.PlasticSCM.Editor.AssetUtils;
using UnityEditor.VersionControl;

namespace Unity.PlasticSCM.Editor.AssetMenu
{
    internal static class AssetsSelection
    {
        internal static Asset GetSelectedAsset(AssetList assetList)
        {
            if (assetList.Count == 0)
                return null;

            return assetList[0];
        }

        internal static string GetSelectedPath(AssetList assetList)
        {
            if (assetList.Count == 0)
                return null;

            return Path.GetFullPath(assetList[0].path);
        }

        internal static List<string> GetSelectedPaths(AssetList selectedAssets)
        {
            List<string> result = new List<string>();

            foreach (Asset asset in selectedAssets)
            {
                string fullPath = Path.GetFullPath(asset.path);
                result.Add(fullPath);
            }

            return result;
        }
    }
}
