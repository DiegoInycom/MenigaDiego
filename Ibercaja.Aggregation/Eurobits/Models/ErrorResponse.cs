using Newtonsoft.Json;

namespace Ibercaja.Aggregation.Eurobits
{
    public class ErrorResponse : IResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("developerMessage")]
        public string DeveloperMessage { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("moreInfoUrl")]
        public string MoreInfoUrl { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }

        public bool HaveData()
        {
            return
                (Code != null) ||
                (DeveloperMessage != null) ||
                (Message != null) ||
                (MoreInfoUrl != null) ||
                (Status != null);
        }
    }
}