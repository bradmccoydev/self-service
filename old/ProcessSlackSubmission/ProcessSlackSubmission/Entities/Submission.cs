using System;
using System.Collections.Generic;

namespace ProcessSlackSubmission.Entities
{
    public class Submission
    {
        public string id { get; set; }
        public string tracking_id { get; set; }
        public string user { get; set; }
        public string team { get; set; }
        public string channel { get; set; }
        public string status { get; set; }
        public string command { get; set; }
        public string payload { get; set; }
        public string endpoint { get; set; }
        public long date_time_unix {get; set;}
        public string date_time_utc { get; set; }
        public List<string> approvers { get; set; }
    }
}
