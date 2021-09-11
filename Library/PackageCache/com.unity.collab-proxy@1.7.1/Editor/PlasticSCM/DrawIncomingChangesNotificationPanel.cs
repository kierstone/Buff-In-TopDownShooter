using UnityEditor;
using UnityEngine;

using Codice.CM.Common;
using PlasticGui;
using PlasticGui.Gluon;
using PlasticGui.WorkspaceWindow.PendingChanges;
using Unity.PlasticSCM.Editor.UI;

using GluonShowIncomingChanges = PlasticGui.Gluon.WorkspaceWindow.ShowIncomingChanges;

namespace Unity.PlasticSCM.Editor
{
    internal class NotificationPanelData
    {
        internal enum StyleType
        {
            None,
            Red,
            Green,
        }

        internal bool HasUpdateAction;
        internal string InfoText;
        internal string ActionText;
        internal string TooltipText;
        internal StyleType NotificationStyle;

        internal void Clear()
        {
            HasUpdateAction = false;
            InfoText = string.Empty;
            ActionText = string.Empty;
            TooltipText = string.Empty;
            NotificationStyle = StyleType.None;
        }
    }

    interface IIncomingChangesNotificationPanel
    {
        bool IsVisible
        {
            get;
        }

        NotificationPanelData Data
        {
            get;
        }
    }

    internal static class DrawIncomingChangesNotificationPanel
    {
        internal static void ForMode(
            WorkspaceInfo workspaceInfo,
            WorkspaceWindow workspaceWindow,
            IMergeViewLauncher mergeViewLauncher,
            IGluonViewSwitcher gluonSwitcher,
            bool isGluonMode,
            bool isVisible,
            NotificationPanelData notificationPanelData)
        {
            if (!isVisible)
                return;

            GUIContent labelContent = new GUIContent(
                notificationPanelData.InfoText, notificationPanelData.TooltipText);
            GUIContent buttonContent = new GUIContent(
                notificationPanelData.ActionText, notificationPanelData.TooltipText);

            float panelWidth = GetPanelWidth(
                labelContent, buttonContent,
                UnityStyles.Notification.Label, EditorStyles.miniButton);

            EditorGUILayout.BeginHorizontal(
                GetStyle(notificationPanelData.NotificationStyle),
                GUILayout.Width(panelWidth));

            GUILayout.Label(labelContent, UnityStyles.Notification.Label);

            DoActionButton(
                workspaceInfo, workspaceWindow,
                mergeViewLauncher, gluonSwitcher, isGluonMode,
                notificationPanelData.HasUpdateAction,
                buttonContent, EditorStyles.miniButton);

            EditorGUILayout.EndHorizontal();
        }

        static void DoActionButton(
            WorkspaceInfo workspaceInfo,
            WorkspaceWindow workspaceWindow,
            IMergeViewLauncher mergeViewLauncher,
            IGluonViewSwitcher gluonSwitcher,
            bool isGluonMode,
            bool isUpdateAction,
            GUIContent buttonContent,
            GUIStyle buttonStyle)
        {
            if (!GUILayout.Button(
                    buttonContent, buttonStyle,
                    GUILayout.ExpandHeight(true),
                    GUILayout.MinWidth(40)))
                return;

            if (isUpdateAction)
            {
                workspaceWindow.UpdateWorkspace();
                return;
            }

            ShowIncomingChangesForMode(
                workspaceInfo, mergeViewLauncher,
                gluonSwitcher, isGluonMode);
        }

        static void ShowIncomingChangesForMode(
            WorkspaceInfo workspaceInfo,
            IMergeViewLauncher mergeViewLauncher,
            IGluonViewSwitcher gluonSwitcher,
            bool isGluonMode)
        {
            if (isGluonMode)
            {
                GluonShowIncomingChanges.FromNotificationBar(
                    workspaceInfo, gluonSwitcher);
                return;
            }

            ShowIncomingChanges.FromNotificationBar(
                workspaceInfo, mergeViewLauncher);
        }

        static GUIStyle GetStyle(
            NotificationPanelData.StyleType styleType)
        {
            if (styleType == NotificationPanelData.StyleType.Red)
                return UnityStyles.Notification.RedNotification;

            return UnityStyles.Notification.GreenNotification;
        }

        static float GetPanelWidth(
            GUIContent labelContent, GUIContent buttonContent,
            GUIStyle labelStyle, GUIStyle buttonStyle)
        {
            return labelStyle.CalcSize(labelContent).x +
                buttonStyle.CalcSize(buttonContent).x + 12;
        }
    }
}
