using Newtonsoft.Json;
using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    public class TokenExchangeResponse
    {
        [JsonProperty("error")]
        public ErrorResponse.ErrorFields Error { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
