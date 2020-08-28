using Newtonsoft.Json;

namespace SlackSlashCommand.Entities
{
    public sealed class SlashCommand
    {
        public class Body
        {
            [JsonProperty("token")]
            public string Token { get; set; }

            [JsonProperty("team_id")]
            public string TeamId { get; set; }

            [JsonProperty("team_domain")]
            public string TeamDomain { get; set; }

            [JsonProperty("channel_id")]
            public string ChannelId { get; set; }

            [JsonProperty("channel_name")]
            public string ChannelName { get; set; }

            [JsonProperty("user_id")]
            public string UserId { get; set; }

            [JsonProperty("user_name")]
            public string UserName { get; set; }

            [JsonProperty("command")]
            public string Command { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("api_app_id")]
            public string ApiAppId { get; set; }

            [JsonProperty("response_url")]
            public string ResponseUrl { get; set; }

            [JsonProperty("trigger_id")]
            public string TriggerId { get; set; }
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
            public Headers headers { get; set; }
        }


    }
}