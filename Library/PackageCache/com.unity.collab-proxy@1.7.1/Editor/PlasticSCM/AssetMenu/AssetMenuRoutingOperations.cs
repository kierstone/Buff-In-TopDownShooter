using UnityEditor;

namespace Unity.PlasticSCM.Editor.AssetMenu
{
    internal class AssetMenuRoutingOperations: IAssetMenuOperations
    {
        void IAssetMenuOperations.ShowPendingChanges()
        {
            PlasticWindow plasticWindow = EditorWindow.GetWindow<PlasticWindow>();
            plasticWindow.ShowPendingChanges();
        }

        void IAssetMenuOperations.Add()
        {
            PlasticWindow plasticWindow = EditorWindow.GetWindow<PlasticWindow>();
            plasticWindow.Add();
        }

        void IAssetMenuOperations.Checkout()
        {
            PlasticWindow plasticWindow = EditorWindow.GetWindow<PlasticWindow>();
            plasticWindow.Checkout();
        }

        void IAssetMenuOperations.Checkin()
        {
            PlasticWindow plasticWindow = EditorWindow.GetWindow<PlasticWindow>();
            plasticWindow.Checkin();
        }

        void IAssetMenuOperations.Undo()
        {
            PlasticWindow plasticWindow = EditorWindow.GetWindow<PlasticWindow>();
            plasticWindow.Undo();
        }

        void IAssetMenuOperations.ShowDiff()
        {
            PlasticWindow plasticWindow = EditorWindow.GetWindow<PlasticWindow>();
            plasticWindow.ShowDiff();
        }

        void IAssetMenuOperations.ShowHistory()
        {
            PlasticWindow plasticWindow = EditorWindow.GetWindow<PlasticWindow>();
            plasticWindow.ShowHistory();
        }
    }
}