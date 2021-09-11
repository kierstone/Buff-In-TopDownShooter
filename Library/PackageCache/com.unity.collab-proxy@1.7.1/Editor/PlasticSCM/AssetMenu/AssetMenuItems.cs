using UnityEditor;
using UnityEngine;

using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.Inspector;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.AssetMenu
{
    [InitializeOnLoad]
    internal class AssetMenuItems
    {
        // Adds about 500ms to startup time
        static AssetMenuItems()
        {
            PlasticApp.InitializeIfNeeded();
            sPlasticAPI = new PlasticAPI();

            Enable();
        }

        // TODO: do this after calling plastic workspace
        static void Enable()
        {
            WorkspaceInfo wkInfo = FindWorkspace.InfoForApplicationPath(
                Application.dataPath,
                sPlasticAPI);

            if (wkInfo == null)
            {
                return;
            }

            sOperations = new AssetMenuRoutingOperations();
            sAssetStatusCache = new AssetStatusCache(
                wkInfo,
                sPlasticAPI.IsGluonWorkspace(wkInfo),
                RepaintProjectWindow);
            sAssetSelection = new InspectorAssetSelection();

            AddMenuItems();
        }

        // TODO: Call this from the delete plastic workspace window
        internal static void Disable()
        {
            RemoveMenuItems();
        }


        static void AddMenuItems()
        {
            // TODO: Try removing this
            // Somehow first item always disappears. So this is a filler item
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.PendingChangesPlasticMenu),
                PENDING_CHANGES_MENU_ITEM_PRIORITY,
                PendingChanges, ValidatePendingChanges);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.PendingChangesPlasticMenu),
                PENDING_CHANGES_MENU_ITEM_PRIORITY,
                PendingChanges, ValidatePendingChanges);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.AddPlasticMenu),
                ADD_MENU_ITEM_PRIORITY,
                Add, ValidateAdd);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.CheckoutPlasticMenu),
                CHECKOUT_MENU_ITEM_PRIORITY,
                Checkout, ValidateCheckout);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.CheckinPlasticMenu),
                CHECKIN_MENU_ITEM_PRIORITY,
                Checkin, ValidateCheckin);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.UndoPlasticMenu),
                UNDO_MENU_ITEM_PRIORITY,
                Undo, ValidateUndo);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.DiffPlasticMenu),
                GetPlasticShortcut.ForAssetDiff(),
                DIFF_MENU_ITEM_PRIORITY,
                Diff, ValidateDiff);
            HandleMenuItem.AddMenuItem(
                GetPlasticMenuItemName(PlasticLocalization.Name.HistoryPlasticMenu),
                GetPlasticShortcut.ForHistory(),
                HISTORY_MENU_ITEM_PRIORITY,
                History, ValidateHistory);

            HandleMenuItem.UpdateAllMenus();
        }

        static string GetPlasticMenuItemName(PlasticLocalization.Name name)
        {
            return string.Format("{0}/{1}",
                PlasticLocalization.GetString(PlasticLocalization.Name.PrefixPlasticMenu),
                PlasticLocalization.GetString(name));
        }

        static void PendingChanges()
        {
            ShowWindow.Plastic();

            sOperations.ShowPendingChanges();
        }

        static bool ValidatePendingChanges()
        {
            return true;
        }

        static void Add()
        {
            sOperations.Add();
        }

        static bool ValidateAdd()
        {
            return ShouldMenuItemBeEnabled(AssetMenuOperations.Add);
        }

        static void Checkout()
        {
            sOperations.Checkout();
        }

        static bool ValidateCheckout()
        {
            return ShouldMenuItemBeEnabled(AssetMenuOperations.Checkout);
        }

        static void Checkin()
        {
            sOperations.Checkin();
        }

        static bool ValidateCheckin()
        {
            return ShouldMenuItemBeEnabled(AssetMenuOperations.Checkin);
        }

        static void Undo()
        {
            sOperations.Undo();
        }

        static bool ValidateUndo()
        {
            return ShouldMenuItemBeEnabled(AssetMenuOperations.Undo);
        }

        static void Diff()
        {
            sOperations.ShowDiff();
        }

        static bool ValidateDiff()
        {
            return ShouldMenuItemBeEnabled(AssetMenuOperations.Diff);
        }

        static void History()
        {
            ShowWindow.Plastic();

            sOperations.ShowHistory();
        }

        static bool ValidateHistory()
        {
            return ShouldMenuItemBeEnabled(AssetMenuOperations.History);
        }

        static bool ShouldMenuItemBeEnabled(AssetMenuOperations operation)
        {
            if (sOperations == null)
                return false;

            SelectedAssetGroupInfo selectedGroupInfo = SelectedAssetGroupInfo.
                BuildFromAssetList(
                    sAssetSelection.GetSelectedAssets(),
                    sAssetStatusCache);

            AssetMenuOperations operations = AssetMenuUpdater.
                GetAvailableMenuOperations(selectedGroupInfo);

            return operations.HasFlag(operation);
        }

        static void RepaintProjectWindow()
        {
            EditorWindow projectWindow = FindEditorWindow.ProjectWindow();

            if (projectWindow == null)
                return;

            projectWindow.Repaint();
        }

        static void RemoveMenuItems()
        {
            HandleMenuItem.RemoveMenuItem(
                PlasticLocalization.GetString(PlasticLocalization.Name.PrefixPlasticMenu));

            HandleMenuItem.UpdateAllMenus();
        }

        static PlasticAPI sPlasticAPI;
        static IAssetMenuOperations sOperations;
        static IAssetStatusCache sAssetStatusCache;
        static AssetOperations.IAssetSelection sAssetSelection;

        const int BASE_MENU_ITEM_PRIORITY = 19; // Puts Plastic SCM right below Create menu

        // incrementing the "order" param by 11 causes the menu system to add a separator
        const int PENDING_CHANGES_MENU_ITEM_PRIORITY = BASE_MENU_ITEM_PRIORITY;
        const int ADD_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 11;
        const int CHECKOUT_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 12;
        const int CHECKIN_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 13;
        const int UNDO_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 14;
        const int DIFF_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 25;
        const int HISTORY_MENU_ITEM_PRIORITY = PENDING_CHANGES_MENU_ITEM_PRIORITY + 26;
    }
}