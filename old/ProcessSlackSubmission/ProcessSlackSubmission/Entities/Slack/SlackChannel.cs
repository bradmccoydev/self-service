using System.Collections.Generic;

namespace ProcessSlackSubmission.Entities
{
    public class SlackChannel
    {
        public class Topic
        {
            public string value { get; set; }
            public string creator { get; set; }
            public int last_set { get; set; }
        }

        public class Purpose
        {
            public string value { get; set; }
            public string creator { get; set; }
            public int last_set { get; set; }
        }

        public class Channel
        {
            public string id { get; set; }
            public string name { get; set; }
            public bool is_channel { get; set; }
            public int created { get; set; }
            public bool is_archived { get; set; }
            public bool is_general { get; set; }
            public int unlinked { get; set; }
            public string creator { get; set; }
            public string name_normalized { get; set; }
            public bool is_shared { get; set; }
            public bool is_org_shared { get; set; }
            public bool is_member { get; set; }
            public bool is_private { get; set; }
            public bool is_mpim { get; set; }
            public List<object> members { get; set; }
            public Topic topic { get; set; }
            public Purpose purpose { get; set; }
            public List<object> previous_names { get; set; }
            public int num_members { get; set; }
        }

        public class ResponseMetadata
        {
            public List<string> warnings { get; set; }
        }

        public class RootObject
        {
            public bool ok { get; set; }
            public List<Channel> channels { get; set; }
            public string warning { get; set; }
            public ResponseMetadata response_metadata { get; set; }
        }
    }
}
