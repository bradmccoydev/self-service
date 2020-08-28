using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlackSlashCommand.Entities
{
    public sealed class SlackReply
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("attachments")]
        public List<Dictionary<string, string>> Attachments { get; set; }
    }
}