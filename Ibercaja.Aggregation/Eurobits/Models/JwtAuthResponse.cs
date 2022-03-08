using Newtonsoft.Json;

namespace Ibercaja.Aggregation.Eurobits
{
    public class JwtAuthResponse : IResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
