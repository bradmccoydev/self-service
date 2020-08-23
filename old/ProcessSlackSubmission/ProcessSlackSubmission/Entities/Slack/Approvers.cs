
using System.Collections.Generic;

namespace ProcessSlackSubmission.Entities
{
    public class Approvers
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public List<string> groups { get; set; }
    }
}