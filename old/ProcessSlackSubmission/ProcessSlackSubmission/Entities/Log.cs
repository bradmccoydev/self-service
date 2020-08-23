using System;
namespace ProcessSlackSubmission.Entities
{
    public class Log
    {
        public string job_id { get; set; }
        public string arn { get; set; }
        public string step { get; set; }
        public string message { get; set; }
        public string date_time_utc { get; set; }
    }
}
