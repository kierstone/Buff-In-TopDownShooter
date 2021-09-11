using Codice.Client.Common;
using Codice.Client.Common.WebApi;
using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.WebApi;

namespace Unity.PlasticSCM.Editor.ProjectDownloader
{
    internal static class AutoConfigClientConf
    {
        internal static void FromUnityAccessToken(
            string unityAccessToken,
            RepositorySpec repSpec,
            string projectPath)
        {
            CredentialsResponse response =
                PlasticScmRestApiClient.GetCredentials(unityAccessToken);

            if (response.Error != null)
            {
                UnityEngine.Debug.LogErrorFormat(
                    PlasticLocalization.GetString(PlasticLocalization.Name.ErrorGettingCredentialsCloudProject),
                    response.Error.Message,
                    response.Error.ErrorCode);

                return;
            }

            ClientConfigData configData = BuildClientConfigData(
                repSpec,
                projectPath,
                response);

            ClientConfig.Get().Save(configData);
        }

        static ClientConfigData BuildClientConfigData(
            RepositorySpec repSpec,
            string projectPath,
            CredentialsResponse response)
        {
            SEIDWorkingMode workingMode = SEIDWorkingMode.LDAPWorkingMode;

            ClientConfigData configData = new ClientConfigData();

            configData.WorkspaceServer = repSpec.Server;
            configData.CurrentWorkspace = projectPath;
            configData.WorkingMode = workingMode.ToString();
            configData.SecurityConfig = UserInfo.GetSecurityConfigStr(
                workingMode,
                response.Email,
                GetPassword(response.Token, response.Type));
            return configData;
        }

        static string GetPassword(
            string token,
            CredentialsResponse.TokenType tokenType)
        {
            if (tokenType == CredentialsResponse.TokenType.Bearer)
                return BEARER_PREFIX + token;

            return token;
        }

        const string BEARER_PREFIX = "Bearer ";
    }
}
