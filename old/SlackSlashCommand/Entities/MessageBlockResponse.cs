namespace SlackSlashCommand.Entities
{
    public class MessageBlockResponse
    {
        public class RootObject
        {
            public bool ok { get; set; }
            public string message_ts { get; set; }
        }
    }
}
