using Newtonsoft.Json;

namespace Ibercaja.Aggregation.Eurobits
{
    public class RobotDetailsResponse : IResponse
    {
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public string[] Descriptions { get; set; }
        [JsonProperty(PropertyName = "dynamicParam")]
        public DynamicParam DynamicParam { get; set; }
        [JsonProperty(PropertyName = "globalParameters")]
        public GlobalParameters GlobalParameters { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "products")]
        public Product[] Products { get; set; }
        [JsonProperty(PropertyName = "robotLoginErrors")]
        public ErrorInfo[] RobotLoginErrors { get; set; }
    }

    public class ErrorInfo
    {
        [JsonProperty(PropertyName = "caseId")]
        public string CaseId { get; set; }
        [JsonProperty(PropertyName = "involving")]
        public string Involving { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class DynamicParam
    {
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class GlobalParameters
    {
        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public string[] Descriptions { get; set; }
        [JsonProperty(PropertyName = "encoded")]
        public bool Encoded { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }
        [JsonProperty(PropertyName = "params")]
        public Param[] Params { get; set; }
        [JsonProperty(PropertyName = "paramsValue")]
        public ParamValueInfo[] ParamsValue { get; set; }
        [JsonProperty(PropertyName = "plainParams")]
        public string[] PlainParams { get; set; }
        [JsonProperty(PropertyName = "required")]
        public bool Required { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class ParamValueInfo
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class Param
    {
        [JsonProperty(PropertyName = "defaultValue")]
        public string DefaultValue { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public object Descriptions { get; set; }
        [JsonProperty(PropertyName = "encoded")]
        public bool Encoded { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }
        [JsonProperty(PropertyName = "paramsValue")]
        public ParamsValue[] ParamsValue { get; set; }
        [JsonProperty(PropertyName = "plainParams")]
        public object[] PlainParams { get; set; }
        [JsonProperty(PropertyName = "required")]
        public bool Required { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class ParamsValue
    {
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }
        [JsonProperty(PropertyName = "value")]
        public string
            value
        { get; set; }
    }

    public class Product
    {
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public string[] Descriptions { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "productErrors")]
        public ErrorInfo[] ProductErrors { get; set; }
        [JsonProperty(PropertyName = "subProducts")]
        public SubProduct[] SubProducts { get; set; }
    }

    public class SubProduct
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "subProductErrors")]
        public ErrorInfo[] SubProductErrors { get; set; }
    }
}
