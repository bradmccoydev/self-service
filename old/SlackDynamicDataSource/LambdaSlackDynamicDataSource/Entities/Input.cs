using Newtonsoft.Json;

namespace LambdaSlackDynamicDataSource.Entities
{
    public sealed class Input
    {
        public class Headers
        {
            [JsonProperty("X-Slack-Request-Timestamp")]
            public string SlackRequestTimestamp { get; set; }

            [JsonProperty("X-Slack-Signature")]
            public string SlackSignature { get; set; }
        }

        public class RootObject
        {
            public string body { get; set; }
            public Headers headers { get; set; }
        }
    }
}
