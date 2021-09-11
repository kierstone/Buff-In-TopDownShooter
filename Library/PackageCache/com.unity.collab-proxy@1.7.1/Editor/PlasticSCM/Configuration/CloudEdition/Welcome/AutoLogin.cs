using Codice.Client.Common.Servers;
using Codice.Client.Common.Threading;
using Codice.LogWrapper;
using PlasticGui.Configuration.CloudEdition.Welcome;
using PlasticGui.WebApi;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Configuration.CloudEdition.Welcome
{
    internal class AutoLogin : OAuthSignIn.INotify
    {
        void OAuthSignIn.INotify.SuccessForConfigure(
            List<string> organizations,
            bool canCreateAnOrganization,
            string userName,
            string accessToken)
        {
            ChooseOrganization(organizations, canCreateAnOrganization);
        }

        void OAuthSignIn.INotify.SuccessForSSO(string organization)
        {
        }

        void OAuthSignIn.INotify.SuccessForProfile(string email)
        {
        }

        void OAuthSignIn.INotify.SuccessForCredentials(
            string email,
            string accessToken)
        {
        }

        void OAuthSignIn.INotify.Cancel(string errorMessage)
        {
        }

        internal void Run()
        {
            mPlasticWindow = GetPlasticWindow();

            if (!string.IsNullOrEmpty(CloudProjectSettings.accessToken))
            {
                ExchangeTokensAndJoinOrganization(CloudProjectSettings.accessToken);
            }
        }

        void ExchangeTokensAndJoinOrganization(string unityAccessToken)
        {
            int ini = Environment.TickCount;

            TokenExchangeResponse response = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
            /*threadOperationDelegate*/ delegate
            {
                response = PlasticScmRestApiClient.TokenExchange(unityAccessToken);
            },
            /*afterOperationDelegate*/ delegate
            {
                mLog.DebugFormat(
                    "TokenExchange time {0} ms",
                    Environment.TickCount - ini);

                if (waiter.Exception != null)
                {
                    ExceptionsHandler.LogException(
                        "TokenExchangeSetting",
                        waiter.Exception);
                    return;
                }

                if (response == null)
                {
                    Debug.Log("response null");
                    return;
                }
                   
                if (response.Error != null)
                {
                    mLog.ErrorFormat(
                        "Unable to exchange token: {0} [code {1}]",
                        response.Error.Message, response.Error.ErrorCode);
                    return;
                }

                if (string.IsNullOrEmpty(response.AccessToken))
                {
                    mLog.InfoFormat(
                        "Access token is empty for user: {0}",
                        response.User);
                    return;
                }

                sAccessToken = response.AccessToken;
                sUserName = response.User;
                GetOrganizationList();
            });
        }

        internal void ExchangeTokens(string unityAccessToken)
        {
            int ini = Environment.TickCount;

            TokenExchangeResponse response = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(10);
            waiter.Execute(
            /*threadOperationDelegate*/ delegate
            {
                response = PlasticScmRestApiClient.TokenExchange(unityAccessToken);
            },
            /*afterOperationDelegate*/ delegate
        {
            mLog.DebugFormat(
                "TokenExchange time {0} ms",
                Environment.TickCount - ini);

            if (waiter.Exception != null)
            {
                ExceptionsHandler.LogException(
                    "TokenExchangeSetting",
                    waiter.Exception);
                return;
            }

            if (response == null)
            {
                Debug.Log("response null");
                return;
            }

            if (response.Error != null)
            {
                mLog.ErrorFormat(
                    "Unable to exchange token: {0} [code {1}]",
                    response.Error.Message, response.Error.ErrorCode);
                return;
            }

            if (string.IsNullOrEmpty(response.AccessToken))
            {
                mLog.InfoFormat(
                    "Access token is empty for user: {0}",
                    response.User);
                return;
            }

            sAccessToken = response.AccessToken;
            sUserName = response.User;
        });
        }

        internal void GetOrganizationList()
        {
            OAuthSignIn.GetOrganizationsFromAccessToken(
                mPlasticWindow.PlasticWebRestApiForTesting,
                new Editor.UI.Progress.ProgressControlsForDialogs(),
                this,
                CloudProjectSettings.userName,
                sAccessToken
            );
        }

        PlasticWindow GetPlasticWindow()
        {
            var windows = Resources.FindObjectsOfTypeAll<PlasticWindow>();
            PlasticWindow plasticWindow = windows.Length > 0 ? windows[0] : null;

            if (plasticWindow == null)
                plasticWindow = ShowWindow.Plastic();

            return plasticWindow;
        }

        internal void ChooseOrganization(List<string> organizations,
            bool canCreateAnOrganization)
        {
            mPlasticWindow = GetPlasticWindow();

            CloudEditionWelcomeWindow.ShowWindow(
                   mPlasticWindow.PlasticWebRestApiForTesting,
                   mPlasticWindow.CmConnectionForTesting,null, true);

            mCloudEditionWelcomeWindow = CloudEditionWelcomeWindow.GetWelcomeWindow();
            mCloudEditionWelcomeWindow.FillUserAndToken(sUserName, sAccessToken);
            if (organizations.Count == 1)
            {
                mCloudEditionWelcomeWindow.JoinOrganizationAndWelcomePage(organizations[0]);
                return;
            }
            mCloudEditionWelcomeWindow.ShowOrganizationPanelFromAutoLogin(organizations, canCreateAnOrganization);
        }

        internal static string sAccessToken = string.Empty;
        internal static string sUserName = string.Empty;
        PlasticWindow mPlasticWindow;
        CloudEditionWelcomeWindow mCloudEditionWelcomeWindow;
        static readonly ILog mLog = LogManager.GetLogger("TokensExchange");
    }

}
