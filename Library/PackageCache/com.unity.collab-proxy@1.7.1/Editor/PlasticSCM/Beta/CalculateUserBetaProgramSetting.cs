using System;

using UnityEditor;

using Codice.Client.Common.Threading;
using Codice.LogWrapper;
using Unity.PlasticSCM.Editor.WebApi;

namespace Unity.PlasticSCM.Editor.Beta
{
    [InitializeOnLoad]
    internal static class CalculateUserBetaProgramSetting
    {
        static CalculateUserBetaProgramSetting()
        {
            EditorApplication.update += RunOnceWhenAccessTokenIsInitialized;
        }

        static void RunOnceWhenAccessTokenIsInitialized()
        {
            if (string.IsNullOrEmpty(CloudProjectSettings.accessToken))
                return;

            if (CollabPlugin.IsEnabled())
                return;

            Execute(CloudProjectSettings.accessToken);

            EditorApplication.update -= RunOnceWhenAccessTokenIsInitialized;
        }

        static void Execute(string unityAccessToken)
        {
            if (SessionState.GetInt(
                    IS_USER_BETA_PROGRAM_ALREADY_CALCULATED_KEY,
                    BETA_PROGRAM_NOT_CALCULATED) == BETA_PROGRAM_ENABLED)
            {
                PlasticMenuItem.Add();
                return;
            }

            PlasticApp.InitializeIfNeeded();

            EnableUserBetaProgramIfNeeded(unityAccessToken);
        }

        static void EnableUserBetaProgramIfNeeded(string unityAccessToken)
        {
            int ini = Environment.TickCount;

            UnityPackageBetaEnrollResponse response = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
            /*threadOperationDelegate*/ delegate
            {
                response = PlasticScmRestApiClient.IsBetaEnabled(unityAccessToken);
            },
            /*afterOperationDelegate*/ delegate
            {
                mLog.DebugFormat(
                    "IsBetaEnabled time {0} ms",
                    Environment.TickCount - ini);

                if (waiter.Exception != null)
                {
                    ExceptionsHandler.LogException(
                        "CalculateUserBetaProgramSetting",
                        waiter.Exception);

                    SetBetaProgramNotEnabled();

                    return;
                }

                if (response == null)
                {
                    SetBetaProgramNotEnabled();

                    return;
                }

                if (response.Error != null)
                {
                    mLog.ErrorFormat(
                        "Unable to retrieve is beta enabled: {0} [code {1}]",
                        response.Error.Message,
                        response.Error.ErrorCode);

                    SetBetaProgramNotEnabled();

                    return;
                }

                if (!response.IsBetaEnabled)
                {
                    mLog.InfoFormat(
                        "Beta is disabled for accessToken: {0}",
                        unityAccessToken);

                    SetBetaProgramNotEnabled();

                    return;
                }

                SessionState.SetInt(
                    IS_USER_BETA_PROGRAM_ALREADY_CALCULATED_KEY,
                    BETA_PROGRAM_ENABLED);

                PlasticMenuItem.Add();
            });
        }

        static void SetBetaProgramNotEnabled()
        {
            SessionState.SetInt(
                IS_USER_BETA_PROGRAM_ALREADY_CALCULATED_KEY,
                BETA_PROGRAM_NOT_ENABLED);
        }

        const string IS_USER_BETA_PROGRAM_ALREADY_CALCULATED_KEY =
            "PlasticSCM.UserBetaProgram.IsAlreadyCalculated";

        const int BETA_PROGRAM_NOT_CALCULATED = 0;
        const int BETA_PROGRAM_NOT_ENABLED = 1;
        const int BETA_PROGRAM_ENABLED = 2;

        static readonly ILog mLog = LogManager.GetLogger("CalculateUserBetaProgramSetting");
    }
}
