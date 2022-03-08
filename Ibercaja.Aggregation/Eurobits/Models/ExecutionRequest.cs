using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ibercaja.Aggregation.Eurobits
{
    public class ExecutionRequest : IRequest
    {
        [JsonProperty("robotName")]
        public string RobotName { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("fromDate")]
        public string FromDate { get; set; }
        [JsonProperty("toDate")]
        public string ToDate { get; set; }
        [JsonProperty("products")]
        public string[] Products { get; set; }
        [JsonProperty("loginParameters")]
        public Dictionary<string,string> LoginParameters { get; set; }
        [JsonProperty("extendedTrxData")]
        public bool ExtendedTrxData { get; set; }
        [JsonProperty("encryptedCredentials")]
        public bool EncryptedCredentials { get; set; }
        [JsonProperty("certificateId")]
        public string CertificateId { get; set; }
    }
}