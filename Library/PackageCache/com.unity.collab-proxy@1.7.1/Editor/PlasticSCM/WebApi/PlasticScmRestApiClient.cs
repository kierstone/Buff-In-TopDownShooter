using System;
using System.IO;
using System.Net;

using Newtonsoft.Json;

using Codice.Client.Common.WebApi;
using Codice.CM.Common;
using Codice.LogWrapper;
using PlasticGui.Help.NewVersions;
using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    internal static class PlasticScmRestApiClient
    {
        internal static UnityPackageBetaEnrollResponse IsBetaEnabled(string bearerToken)
        {
            Uri endpoint = mWebApiUris.GetFullUri(IsBetaEnabledEndpoint);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(
                    HttpRequestHeader.Authorization,
                    string.Format("Bearer {0}", bearerToken));

                return GetResponse<UnityPackageBetaEnrollResponse>(request);
            }
            catch (Exception ex)
            {
                mLog.ErrorFormat(
                    "Unable to retrieve is beta enabled '{0}': {1}",
                    endpoint.ToString(), ex.Message);

                mLog.DebugFormat(
                    "StackTrace:{0}{1}",
                    Environment.NewLine, ex.StackTrace);

                return null;
            }
        }

        internal static TokenExchangeResponse TokenExchange(string unityAccessToken)
        {
            Uri endpoint = mWebApiUris.GetFullUri(
                string.Format("{0}/{1}",TokenExchangeEndpoint,unityAccessToken ));

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
                request.Method = "GET";
                request.ContentType = "application/json";
                return GetResponse<TokenExchangeResponse>(request);
            }
            catch (Exception ex)
            {
                mLog.ErrorFormat(
                    "Unable to exchange tokens '{0}': {1}",
                    endpoint.ToString(), ex.Message);

                mLog.DebugFormat(
                    "StackTrace:{0}{1}",
                    Environment.NewLine, ex.StackTrace);

                return null;
            }
        }

        internal static NewVersionResponse GetLastVersion(Edition plasticEdition)
        {
            Uri endpoint = mWebApiUris.GetFullUri(
                    WebApiEndpoints.LastVersion.NewVersion,
                    "9.0.0.0",
                    WebApiEndpoints.LastVersion.GetEditionString(plasticEdition),
                    WebApiEndpoints.LastVersion.GetPlatformString());

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
                request.Method = "GET";
                request.ContentType = "application/json";

                return GetResponse<NewVersionResponse>(request);
            }
            catch (Exception ex)
            {
                mLog.ErrorFormat(
                    "Unable to retrieve new versions from '{0}': {1}",
                    endpoint.ToString(), ex.Message);

                mLog.DebugFormat(
                    "StackTrace:{0}{1}",
                    Environment.NewLine, ex.StackTrace);

                return null;
            }
        }

        internal static CredentialsResponse GetCredentials(string unityToken)
        {
            Uri endpoint = mWebApiUris.GetFullUri(
                WebApiEndpoints.Authentication.Credentials,
                unityToken);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
                request.Method = "GET";
                request.ContentType = "application/json";

                return GetResponse<CredentialsResponse>(request);
            }
            catch (Exception ex)
            {
                return new CredentialsResponse
                {
                    Error = BuildLoggedErrorFields(ex, endpoint)
                };
            }
        }

        static TRes GetResponse<TRes>(WebRequest request)
        {
            using (WebResponse response = request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();

                if (string.IsNullOrEmpty(json))
                    return default(TRes);

                return JsonConvert.DeserializeObject<TRes>(json);
            }
        }

        static ErrorResponse.ErrorFields BuildLoggedErrorFields(
            Exception ex, Uri endpoint)
        {
            LogException(ex, endpoint);

            return new ErrorResponse.ErrorFields
            {
                ErrorCode = ErrorCodes.ClientError,
                Message = ex.Message
            };
        }

        static void LogException(Exception ex, Uri endpoint)
        {
            mLog.ErrorFormat(
                "There was an error while calling '{0}': {1}",
                endpoint.ToString(), ex.Message);

            mLog.DebugFormat(
                "StackTrace:{0}{1}",
                Environment.NewLine, ex.StackTrace);
        }

        const string IsBetaEnabledEndpoint = "api/unity-package/beta/is-enabled";
        const string TokenExchangeEndpoint = "api/oauth/unityid/exchange";
        static readonly PlasticWebApiUris mWebApiUris = PlasticWebApiUris.BuildDefault();
        static readonly ILog mLog = LogManager.GetLogger("PlasticScmRestApiClient");
    }
}
