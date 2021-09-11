using Unity.PlasticSCM.Editor.AssetMenu;
using Unity.PlasticSCM.Editor.AssetUtils;
using UnityEditor.VersionControl;

namespace Unity.PlasticSCM.Editor.Inspector
{
    internal class InspectorAssetSelection : AssetOperations.IAssetSelection
    {
        AssetList AssetOperations.IAssetSelection.GetSelectedAssets()
        {
            AssetList result = new AssetList();

            foreach (UnityEngine.Object obj in UnityEditor.Selection.objects)
            {
                result.Add(new Asset(AssetsPath.GetFullPath(obj)));
            }

            return result;
        }
    }
}
