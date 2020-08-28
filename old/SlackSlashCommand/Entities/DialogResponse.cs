using System.Collections.Generic;

namespace SlackSlashCommand.Entities
{
    public class DialogResponse
    {
        public class ResponseMetadata
        {
            public List<string> warnings { get; set; }
        }

        public class RootObject
        {
            public bool ok { get; set; }
            public string error { get; set; }
            public string warning { get; set; }
            public ResponseMetadata response_metadata { get; set; }
        }
    }
}
