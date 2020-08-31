using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace LambdaSlackDynamicDataSource.Authorization
{
    public class AwsSecretsManagerServiceAccessor
    {
        public async Task<string> GetSecretValueAsync(
           string accessKey,
           string secretKey,
           string region,
           string secretId)
        {
            AmazonSecretsManagerClient client = new AmazonSecretsManagerClient(
                 accessKey,
                 secretKey,
                 RegionEndpoint.USWest2);

            var response = await client.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secretId
            });

            return response.SecretString;
        }
    }
}
