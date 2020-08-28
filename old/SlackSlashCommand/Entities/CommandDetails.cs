using Newtonsoft.Json;
using System.Collections.Generic;

namespace SlackSlashCommand.Entities
{
    public sealed class CommandDetails
    {
        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("preState")]
        public string PreState { get; set; }

        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }

        [JsonProperty("dialog")]
        public string Dialog { get; set; }


        [JsonProperty("job_id")]
        public string JobId { get; set; }
        
        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("block_action")]
        public string BlockAction { get; set; }

        [JsonProperty("requires_authorization")]
        public string RequiresAuthorization { get; set; }

        [JsonProperty("requires_approval")]
        public string RequiresApproval { get; set; }

        [JsonProperty("authorised_users")]
        public string AuthorisedUsers { get; set; }

        [JsonProperty("authorised_groups")]
        public string AuthorisedGroups { get; set; }

        [JsonProperty("approver_users")]
        public string ApproverUsers { get; set; }

        [JsonProperty("ApproverGroups")]
        public string ApproverGroups { get; set; }
    }
}