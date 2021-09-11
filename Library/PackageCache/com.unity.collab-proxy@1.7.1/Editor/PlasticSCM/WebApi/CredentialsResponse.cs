using System.Reflection;

using Newtonsoft.Json;

using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    [ObfuscationAttribute(Exclude = true)]
    public class CredentialsResponse
    {
        [JsonProperty("error")]
        public ErrorResponse.ErrorFields Error { get; set; }

        public enum TokenType : int
        {
            Password = 0,
            Bearer = 1,
        }

        [JsonIgnore]
        public TokenType Type
        {
            get { return (TokenType)TokenTypeValue; }
        }

        [JsonProperty("email")]
        public string Email;
        [JsonProperty("token")]
        public string Token;
        [JsonProperty("tokenTypeValue")]
        public int TokenTypeValue;
    }
}
