using Newtonsoft.Json;

using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    public class UnityPackageBetaEnrollResponse
    {
        [JsonProperty("error")]
        public ErrorResponse.ErrorFields Error { get; set; }
        [JsonProperty("isBetaEnabled")]
        public bool IsBetaEnabled { get; set; }
    }
}
