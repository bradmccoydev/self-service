using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProcessSlackSubmission.Entities
{
    public class CommandDetails
    {
        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }

        [JsonProperty("created_date")]
        public string CreatedDate { get; set; }

        [JsonProperty("last_updated_by")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("last_updated_date")]
        public string LastUpdatedDate { get; set; }

        [JsonProperty("orchestration_engine")]
        public string OrchestrationEngine { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("preState")]
        public string PreState { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("dialog")]
        public string Dialog { get; set; }

        [JsonProperty("dialog2")]
        public string Dialog2{ get; set; }

        [JsonProperty("external_data_source")]
        public string ExternalDataSource{ get; set; }

        [JsonProperty("request_response")]
        public string RequestResponse { get; set; }

        [JsonProperty("job_id")]
        public string JobId { get; set; }

        [JsonProperty("lambda_name")]
        public string LambdaName { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("authorised_users")]
        public string AuthorisedUsers { get; set; }

        [JsonProperty("authorised_groups")]
        public string AuthorisedGroups { get; set; }

        [JsonProperty("approver_users")]
        public List<string> ApproverUsers { get; set; }

        [JsonProperty("approver_groups")]
        public List<string> ApproverGroups { get; set; }

        [JsonProperty("requires_authorization")]
        public string RequiresAuthorization { get; set; }

        [JsonProperty("requires_approval")]
        public string RequiresApproval { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
