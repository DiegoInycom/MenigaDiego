using Newtonsoft.Json;

namespace Ibercaja.Aggregation.Eurobits
{
    public class UpdateAggregationRequest : IRequest
    {
        [JsonProperty(PropertyName = "pwdVble")]
        public string Parameter { get; set; }
    }
}
