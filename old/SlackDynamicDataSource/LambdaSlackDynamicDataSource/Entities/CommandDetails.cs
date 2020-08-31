using Newtonsoft.Json;

namespace LambdaSlackDynamicDataSource.Entities
{
    public class CommandDetails
    {
        [JsonProperty("external_data_source")]
        public string ExternalDataSource { get; set; }

        [JsonProperty("authorised_users")]
        public string AuthorisedUsers { get; set; }

        [JsonProperty("authorised_groups")]
        public string AuthorisedGroups { get; set; }
    }
}
