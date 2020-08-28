using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using SlackSlashCommand.Entities;
using Newtonsoft.Json;

namespace SlackSlashCommand
{
    public class AwsSecretsManagerServiceAccessor
    {
        public async Task<SlackAppCredentials> GetApplicationSecrets(
           string region,
           string secretId)
        {
            var slackAppCredentialsJson = await GetSecretValueAsync(region: region, secretId: secretId);
            var slackAppCredentials = JsonConvert.DeserializeObject<SlackAppCredentials>(slackAppCredentialsJson);

            return slackAppCredentials;
        }

        public async Task<string> GetSecretValueAsync(
           string region,
           string secretId)
        {
            var client = await GetSecretsManagerClient(
                region);
                
            var response = await client.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secretId
            });

            return response.SecretString;
        }

        public async Task<AmazonSecretsManagerClient> GetSecretsManagerClient(
            string region)
        {
            AmazonSecretsManagerClient secretsManagerClient = new AmazonSecretsManagerClient();

            var iamRegion = RegionEndpoint.GetBySystemName(region);

            secretsManagerClient = new AmazonSecretsManagerClient(iamRegion);

            return secretsManagerClient;

        }
    }
}
