using System;
namespace SlackSlashCommand.Entities
{
    public class SlackDirectMessageChannel
    {
        public class Channel
        {
            public string id { get; set; }
        }

        public class RootObject
        {
            public bool ok { get; set; }
            public Channel channel { get; set; }
        }
    }
}
