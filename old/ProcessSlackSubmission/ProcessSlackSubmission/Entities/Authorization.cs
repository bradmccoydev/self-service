using Newtonsoft.Json;

namespace ProcessSlackSubmission.Entities
{
    public class Authorization
    {
        public class Headers
        {
            [JsonProperty("X-Slack-Request-Timestamp")]
            public string SlackRequestTimestamp { get; set; }

            [JsonProperty("X-Slack-Signature")]
            public string SlackSignature { get; set; }
        }

        public class Body
        {
            public string state { get; set; }
        }

        public class RootObject
        {
            public string raw { get; set; }
            public Headers headers { get; set; }
            public Body body { get; set; }

        }
    }
}
