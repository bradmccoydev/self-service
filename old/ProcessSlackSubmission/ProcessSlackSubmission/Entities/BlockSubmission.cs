using System.Collections.Generic;

namespace ProcessSlackSubmission.Entities
{
    public class BlockSubmission
    {
        public class Team
        {
            public string id { get; set; }
            public string domain { get; set; }
        }

        public class User
        {
            public string id { get; set; }
            public string username { get; set; }
            public string name { get; set; }
            public string team_id { get; set; }
        }

        public class Container
        {
            public string type { get; set; }
            public string message_ts { get; set; }
            public string channel_id { get; set; }
            public bool is_ephemeral { get; set; }
        }

        public class Channel
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Text
        {
            public string type { get; set; }
            public string text { get; set; }
            public bool emoji { get; set; }
        }

        public class Action
        {
            public string action_id { get; set; }
            public string block_id { get; set; }
            public Text text { get; set; }
            public string value { get; set; }
            public string type { get; set; }
            public string style { get; set; }
            public string action_ts { get; set; }
            public string selected_date { get; set; }
        }

        public class Body
        {
            public string type { get; set; }
            public Team team { get; set; }
            public User user { get; set; }
            public string api_app_id { get; set; }
            public string token { get; set; }
            public Container container { get; set; }
            public string trigger_id { get; set; }
            public Channel channel { get; set; }
            public string response_url { get; set; }
            public List<Action> actions { get; set; }
        }

        public class RootObject
        {
            public Body body { get; set; }
        }
    }
}
