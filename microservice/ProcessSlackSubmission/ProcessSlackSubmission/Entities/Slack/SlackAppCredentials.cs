using System;
namespace ProcessSlackSubmission.Entities
{
    public class SlackAppCredentials
    {
        public string AuthToken { get; set; }
        public string BotUserToken { get; set; }
        public string SigningSecret { get; set; }
    }
}
