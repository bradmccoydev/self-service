using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessSlackSubmission.Entities.Aws
{
    public class StsToken
    {
        public string AccessKeyId { get; set; }
        public DateTime Expiration { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
    }
}
