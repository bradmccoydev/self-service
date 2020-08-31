using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System.Threading.Tasks;
using System.Threading;

namespace LambdaSlackDynamicDataSource.Authorization
{
    public class AwsStsServiceAccessor
    {
        public async Task<AssumeRoleResponse> GetStsTokenAsync(
            string accessKey,
            string secretKey,
            string region,
            string roleArn)
        {
            AssumeRoleResponse result = new AssumeRoleResponse();
            CancellationToken token = new CancellationToken();

            var client = await GetStsClient(
                accessKey: accessKey,
                secretKey: secretKey,
                region: region);

            AssumeRoleRequest request = new AssumeRoleRequest();
            request.RoleArn = roleArn;
            request.DurationSeconds = 900;
            request.RoleSessionName = "STSRole";
            request.ExternalId = "AvokaLogCollector";

            return await client.AssumeRoleAsync(
                request: request,
                cancellationToken: token);
        }

        public async Task<AmazonSecurityTokenServiceClient> GetStsClient(
            string accessKey,
            string secretKey,
            string region)
        {
            var awsCreds = new BasicAWSCredentials(
                accessKey, secretKey);

            return new AmazonSecurityTokenServiceClient(
                credentials: awsCreds,
                region: Amazon.RegionEndpoint.USWest2);
        }
    }
}
