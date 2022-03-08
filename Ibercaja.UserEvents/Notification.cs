using System;
using Newtonsoft.Json;

namespace Ibercaja.UserEvents
{
    public class Notification
    {
        [JsonProperty(PropertyName = "nici")]
        public string UserNici { get; set; }
        [JsonProperty(PropertyName = "sourceId")]
        public string SourceId { get; set; }
        [JsonProperty(PropertyName = "notificationType")]
        public string NotificationType { get; set; }
        [JsonProperty(PropertyName = "metaData")]
        public string NotificationMetadata { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string NotificationMessage { get; set; }
        [JsonProperty(PropertyName = "createdEvent")]
        public DateTime CreatedEvent { get; set; }
        [JsonProperty(PropertyName = "createdOn")]
        public DateTime CreatedOn { get; set; }
        [JsonProperty(PropertyName = "sender")]
        public string Sender => "Meniga-PFM";
        [JsonIgnore]
        public string CategoryId { get; set; }
        [JsonIgnore]
        public int AccountTypeId { get; set; }
        [JsonIgnore]
        public string Identifier { get; set; }
    }
}
