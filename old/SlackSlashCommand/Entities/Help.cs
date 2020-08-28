using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlackSlashCommand.Entities
{
    public class Help
    {
        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("aws_secret_manager_id")]
        public string SecretManagerId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("callback_id")]
        public string CallbackId { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }

        [JsonProperty("dialog")]
        public string Dialog { get; set; }

        [JsonProperty("service_arn")]
        public string ServiceArn { get; set; }

        [JsonProperty("request_response")]
        public string RequestResponse { get; set; }

        [JsonProperty("job_id")]
        public string JobId { get; set; }

        [JsonProperty("lambda_name")]
        public string LambdaName { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("step_function_arn")]
        public string StepFunctionArn { get; set; }

        [JsonProperty("role_arn")]
        public string RoleArn { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("authorised_users")]
        public List<string> AuthorisedUsers { get; set; }

        [JsonProperty("authorised_groups")]
        public List<string> AuthorisedGroups { get; set; }

    }
}
