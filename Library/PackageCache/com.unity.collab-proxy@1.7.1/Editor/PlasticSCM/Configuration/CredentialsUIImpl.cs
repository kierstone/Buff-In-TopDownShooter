using UnityEditor;

using Codice.Client.Common;
using Codice.Client.Common.Connection;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor.Configuration
{
    internal class CredentialsUiImpl : AskCredentialsToUser.IGui
    {
        internal CredentialsUiImpl(EditorWindow parentWindow)
        {
            mParentWindow = parentWindow;
        }

        AskCredentialsToUser.DialogData AskCredentialsToUser.IGui.AskUserForCredentials(string servername)
        {
            AskCredentialsToUser.DialogData result = null;

            GUIActionRunner.RunGUIAction(delegate
            {
                result = CredentialsDialog.RequestCredentials(
                        servername, mParentWindow);
            });

            return result;
        }

        void AskCredentialsToUser.IGui.ShowSaveProfileErrorMessage(string message)
        {
            GUIActionRunner.RunGUIAction(delegate
            {
                GuiMessage.ShowError(string.Format(
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.CredentialsErrorSavingProfile),
                    message));
            });
        }

        AskCredentialsToUser.DialogData AskCredentialsToUser.IGui.AskUserForSSOCredentials(string cloudServer)
        {
            AskCredentialsToUser.DialogData result = null;

            GUIActionRunner.RunGUIAction(delegate
            {
                result = SSOCredentialsDialog.RequestCredentials(
                    cloudServer, mParentWindow);
            });

            return result;
        }

        EditorWindow mParentWindow;
    }
}
