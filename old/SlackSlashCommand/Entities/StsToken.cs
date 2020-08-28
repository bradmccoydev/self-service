using System;
namespace SlackSlashCommand.Entities
{
    public class StsToken
    {
        public string AccessKeyId { get; set; }
        public DateTime Expiration { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
    }
}
