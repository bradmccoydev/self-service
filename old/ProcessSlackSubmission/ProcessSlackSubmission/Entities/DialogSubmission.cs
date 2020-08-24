using Newtonsoft.Json;

namespace ProcessSlackSubmission
{
    public class DialogSubmission
    {
        public class Team
        {
            public string id { get; set; }
            public string domain { get; set; }
        }

        public class User
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Channel
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Submission
        {
            public string client_code { get; set; }
        }

        public class Body
        {
            public string type { get; set; }
            public string token { get; set; }
            public string action_ts { get; set; }
            public Team team { get; set; }
            public User user { get; set; }
            public Channel channel { get; set; }
            public Submission submission { get; set; }
            public string callback_id { get; set; }
            public string response_url { get; set; }
            public string state { get; set; }
        }

        public class Headers
        {
            [JsonProperty("X-Slack-Request-Timestamp")]
            public string SlackRequestTimestamp { get; set; }

            [JsonProperty("X-Slack-Signature")]
            public string SlackSignature { get; set; }
        }

        public class RootObject
        {
            public Body body { get; set; }
            public string raw { get; set; }
            public Headers headers { get; set; }
        }
    }
}