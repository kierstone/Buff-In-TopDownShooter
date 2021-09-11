using System;
using System.Reflection;

using UnityEditor;

namespace Unity.PlasticSCM.Editor
{
    internal static class CollabPlugin
    {
        internal static bool IsEnabled()
        {
            return IsCollabInstanceEnabled();
        }

        internal static void Disable()
        {
            DisableCollabInstance();

            DisableCollabInProjectSettings();
        }

        static void DisableCollabInstance()
        {
            object collabInstance = GetCollabInstance();

            if (collabInstance == null)
                return;

            // Invokes Collab.instance.SetCollabEnabledForCurrentProject(false)
            SetCollabEnabledForCurrentProject(collabInstance, false);
        }

        static void DisableCollabInProjectSettings()
        {
            // Invokes PlayerSettings.SetCloudServiceEnabled("Collab", false)
            SetCloudServiceEnabled("Collab", false);

            AssetDatabase.SaveAssets();
        }

        static bool IsCollabInstanceEnabled()
        {
            object collabInstance = GetCollabInstance();

            if (collabInstance == null)
                return false;

            // Invokes Collab.instance.IsCollabEnabledForCurrentProject()
            return IsCollabEnabledForCurrentProject(collabInstance);
        }

        static void SetCollabEnabledForCurrentProject(object collabInstance, bool enable)
        {
            MethodInfo InternalSetCollabEnabledForCurrentProject =
                CollabType.GetMethod("SetCollabEnabledForCurrentProject");

            if (InternalSetCollabEnabledForCurrentProject == null)
                return;

            InternalSetCollabEnabledForCurrentProject.
                Invoke(collabInstance, new object[] { enable });
        }

        static void SetCloudServiceEnabled(string setting, bool enable)
        {
            MethodInfo InternalSetCloudServiceEnabled = PlayerSettingsType.GetMethod(
               "SetCloudServiceEnabled",
               BindingFlags.NonPublic | BindingFlags.Static);

            if (InternalSetCloudServiceEnabled == null)
                return;

            InternalSetCloudServiceEnabled.
                Invoke(null, new object[] { setting, enable });
        }

        static object GetCollabInstance()
        {
            if (CollabType == null)
                return null;

            PropertyInfo InternalInstance =
                CollabType.GetProperty("instance");

            if (InternalInstance == null)
                return null;

            return InternalInstance.GetValue(null, null);
        }

        static bool IsCollabEnabledForCurrentProject(object collabInstance)
        {
            MethodInfo InternalIsCollabEnabledForCurrentProject =
                CollabType.GetMethod("IsCollabEnabledForCurrentProject");

            if (InternalIsCollabEnabledForCurrentProject == null)
                return false;

            return (bool)InternalIsCollabEnabledForCurrentProject.
                Invoke(collabInstance, null);
        }

        static readonly Type CollabType =
            typeof(UnityEditor.Editor).Assembly.
                GetType("UnityEditor.Collaboration.Collab");

        static readonly Type PlayerSettingsType =
            typeof(UnityEditor.PlayerSettings);
    }
}
