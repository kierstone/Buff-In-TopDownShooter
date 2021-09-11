using System;
using System.IO;
using System.Threading;

using UnityEditor;
using UnityEngine;

using Codice.Client.BaseCommands;
using Codice.Client.Commands;
using Codice.Client.Common;
using Codice.CM.Common;
using Codice.LogWrapper;
using Codice.Utils;
using PlasticGui;
using PlasticGui.WebApi;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Update;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.Configuration.CloudEdition.Welcome;

namespace Unity.PlasticSCM.Editor.ProjectDownloader
{
    internal class DownloadRepositoryOperation
    {
        internal void DownloadRepositoryToPathIfNeeded(
            string cloudRepository,
            string cloudOrganization,
            string projectPath,
            string unityAccessToken)
        {
            AssetDatabase.DisallowAutoRefresh();

            try
            {
                BuildProgressSpeedAndRemainingTime.ProgressData progressData =
                    new BuildProgressSpeedAndRemainingTime.ProgressData(DateTime.Now);

                ThreadPool.QueueUserWorkItem(
                    DownloadRepository,
                    new DownloadRepositoryParameters()
                    {
                        CloudOrganization = cloudOrganization,
                        CloudRepository = cloudRepository,
                        ProjectPath = projectPath,
                        AccessToken = unityAccessToken
                    });

                while (!mOperationFinished)
                {
                    if (mDisplayProgress)
                    {
                        DisplayProgress(
                            mUpdateNotifier.GetUpdateStatus(),
                            progressData,
                            cloudRepository);
                    }

                    Thread.Sleep(150);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                AssetDatabase.AllowAutoRefresh();

                RefreshAsset.UnityAssetDatabase();

                if (!mOperationFailed)
                {
                    ShowWindow.PlasticAfterDownloadingProject();
                }
            }
        }

        void DownloadRepository(object state)
        {
            DownloadRepositoryParameters parameters = (DownloadRepositoryParameters)state;

            try
            {
                IPlasticWebRestApi restApi = new PlasticWebRestApi();
                string defaultCloudAlias = restApi.GetDefaultCloudAlias();

                RepositorySpec repSpec = BuildRepSpec(
                    parameters.CloudRepository,
                    parameters.CloudOrganization,
                    defaultCloudAlias);

                // we just download a cloud project,
                // so let's assume we're going to use Cloud Edition
                SetupUnityEditionToken.CreateCloudEditionTokenIfNeeded();

                if (!ClientConfig.IsConfigured())
                {
                    AutoConfigClientConf.FromUnityAccessToken(
                        parameters.AccessToken,
                        repSpec,
                        parameters.ProjectPath);
                }

                if (WorkspaceExists(parameters.ProjectPath))
                {
                    // each domain reload, the package is reloaded.
                    // way need to check if we already downloaded it
                    return;
                }

                mDisplayProgress = true;

                WorkspaceInfo wkInfo = CreateWorkspace(
                    repSpec, parameters.ProjectPath);

                mLog.DebugFormat("Created workspace {0} on {1}",
                    wkInfo.Name,
                    wkInfo.ClientPath);

                AutoLogin autoLogin = new AutoLogin();
                autoLogin.ExchangeTokens(parameters.AccessToken);

                CloudEditionWelcomeWindow.JoinOrganization(parameters.CloudOrganization, AutoLogin.sAccessToken, AutoLogin.sUserName);

                ClientConfigData clientConfigData = ClientConfig.Get().GetClientConfigData();
                clientConfigData.WorkspaceServer = parameters.CloudOrganization;
                ClientConfig.Get().Save(clientConfigData);

                Plastic.API.Update(
                    wkInfo.ClientPath,
                    UpdateFlags.None,
                    null,
                    mUpdateNotifier);
            }
            catch (Exception ex)
            {
                LogException(ex);

                UnityEngine.Debug.LogErrorFormat(
                    PlasticLocalization.GetString(PlasticLocalization.Name.ErrorDownloadingCloudProject),
                    ex.Message);

                mOperationFailed = true;
            }
            finally
            {
                mOperationFinished = true;
            }
        }
    
        static void DisplayProgress(
            UpdateOperationStatus status,
            BuildProgressSpeedAndRemainingTime.ProgressData progressData,
            string cloudRepository)
        {
            string totalProgressMessage = UpdateProgressRender.
                GetProgressString(status, progressData);

            float totalProgressPercent = GetProgressBarPercent.
                ForTransfer(status.UpdatedSize, status.TotalSize) / 100f;

            EditorUtility.DisplayProgressBar(
                string.Format("{0} {1}",
                    PlasticLocalization.GetString(PlasticLocalization.Name.DownloadingProgress),
                    cloudRepository),
                totalProgressMessage, totalProgressPercent);
        }

        static WorkspaceInfo CreateWorkspace(
            RepositorySpec repositorySpec,
            string projectPath)
        {
            CreateWorkspaceDialogUserAssistant assistant = new CreateWorkspaceDialogUserAssistant(
                PlasticGuiConfig.Get().Configuration.DefaultWorkspaceRoot,
                Plastic.API.GetAllWorkspacesArray());

            assistant.RepositoryChanged(
                repositorySpec.ToString(),
                string.Empty,
                string.Empty);

            return Plastic.API.CreateWorkspace(
                projectPath,
                assistant.GetProposedWorkspaceName(),
                repositorySpec.ToString());
        }

        static RepositorySpec BuildRepSpec(
            string cloudRepository,
            string cloudOrganization,
            string defaultCloudAlias)
        {
            return new RepositorySpec()
            {
                Name = cloudRepository,
                Server = CloudServer.BuildFullyQualifiedName(
                    cloudOrganization, defaultCloudAlias)
            };
        }

        static void LogException(Exception ex)
        {
            mLog.WarnFormat("Message: {0}", ex.Message);

            mLog.DebugFormat(
                "StackTrace:{0}{1}",
                Environment.NewLine, ex.StackTrace);
        }

        static bool WorkspaceExists(string projectPath)
        {
            return Plastic.API.GetWorkspaceFromPath(projectPath) != null;
        }

        class DownloadRepositoryParameters
        {
            internal string CloudRepository;
            internal string CloudOrganization;
            internal string ProjectPath;
            internal string AccessToken;
        }

        volatile bool mOperationFinished = false;
        volatile bool mOperationFailed = false;
        volatile bool mDisplayProgress;

        UpdateNotifier mUpdateNotifier = new UpdateNotifier();

        static readonly ILog mLog = LogManager.GetLogger("DownloadRepositoryOperation");
    }
}
