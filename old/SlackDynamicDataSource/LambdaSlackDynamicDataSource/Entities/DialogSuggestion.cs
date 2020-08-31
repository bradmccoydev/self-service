using Newtonsoft.Json;

namespace LambdaSlackDynamicDataSource.Entities
{
    public class DialogSuggestion
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

        public class RootObject
        {
            public string type { get; set; }
            public string token { get; set; }
            public string action_ts { get; set; }
            public Team team { get; set; }
            public User user { get; set; }
            public Channel channel { get; set; }
            public string name { get; set; }
            public string value { get; set; }
            public string callback_id { get; set; }
            public string state { get; set; }
        }
    }
}
