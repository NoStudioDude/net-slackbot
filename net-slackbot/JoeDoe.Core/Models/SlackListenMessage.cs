using Newtonsoft.Json;

namespace JoeDoe.Core.Models
{
    public class SlackListenMessage
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "channel")]
        public string ChannelId { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "team")]
        public string TeamId { get; set; }
    }
}