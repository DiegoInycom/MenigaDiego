using Newtonsoft.Json;

namespace Ibercaja.Aggregation.Eurobits
{
    public class ExecutionResponse : IResponse
    {
        // UUID
        [JsonProperty("executionId")]
        public string ExecutionId { get; set; }
        // yyyy-MM-dd HH:mm:ss
        [JsonProperty("executionTime")]
        public string ExecutionTime { get; set; }
        [JsonProperty("executionUUID")]
        public string ExecutionUUID { get; set; }
    }
}
