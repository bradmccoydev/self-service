using System.Collections.Generic;

namespace ProcessSlackSubmission.Entities
{
    public class SlackGroup
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

        public class Group
        {
            public string id { get; set; }
            public string name { get; set; }
            public bool is_group { get; set; }
            public int created { get; set; }
            public string creator { get; set; }
            public bool is_archived { get; set; }
            public string name_normalized { get; set; }
            public bool is_mpim { get; set; }
            public List<string> members { get; set; }
            public Topic topic { get; set; }
            public Purpose purpose { get; set; }
            public double priority { get; set; }
        }

        public class ResponseMetadata
        {
            public string next_cursor { get; set; }
        }

        public class RootObject
        {
            public bool ok { get; set; }
            public List<Group> groups { get; set; }
            public ResponseMetadata response_metadata { get; set; }
        }
    }
}
