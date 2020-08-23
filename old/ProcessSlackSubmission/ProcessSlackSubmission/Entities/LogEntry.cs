using System;
namespace ProcessSlackSubmission.Entities
{
    public class LogEntry
    {
        public string id { get; set; }
        public string tracking_id { get; set; }
        public string datetime { get; set; }
        public string endpoint { get; set; }
        public string message { get; set; }
        public string step { get; set; }
        public string type { get; set; }
    }
}
