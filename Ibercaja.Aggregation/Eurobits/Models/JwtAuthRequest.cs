using Newtonsoft.Json;

namespace Ibercaja.Aggregation.Eurobits
{
    public class JwtAuthRequest : IRequest
    {
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("service")]
        public string Service { get; set; }
    }
}
