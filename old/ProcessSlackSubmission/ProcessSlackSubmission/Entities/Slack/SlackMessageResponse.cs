
namespace ProcessSlackSubmission.Entities
{
    public class SlackMessageResponse
    {
        public class RootObject
        {
            public bool ok { get; set; }
            public string message_ts { get; set; }
        }
    }
}
