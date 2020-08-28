
namespace SlackSlashCommand.Entities
{
    public class LambdaPayload
    {
        public string TrackingId { get; set; }
        public string ResponseUrl { get; set; }
        public string SlackChannel { get; set; }
        public string SlackUser { get; set; }
    }
}
