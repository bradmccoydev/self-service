using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LambdaSlackDynamicDataSource.Authorization
{
    public class AwsKmsServiceAccessor
    {
        public async Task<string> DecryptDataKeyAsync(
        string accessKey,
        string secretKey,
        string secret)
    {
        var encryptedBase64Text = secret;
        var encryptedBytes = Convert.FromBase64String(encryptedBase64Text);
        string plaintext = "";

        using (var client = new AmazonKeyManagementServiceClient(
            awsAccessKeyId: accessKey,
            awsSecretAccessKey: secretKey,
            region: RegionEndpoint.USWest2))
        {
            var decryptRequest = new DecryptRequest
            {

                CiphertextBlob = new MemoryStream(encryptedBytes),
            };

            var response = await client.DecryptAsync(decryptRequest);
            using (var plaintextStream = response.Plaintext)
            {
                var plaintextBytes = plaintextStream.ToArray();
                plaintext = Encoding.UTF8.GetString(plaintextBytes);
            }
        }

        return plaintext;

    }

    public async Task<string> DecodeEnvVar(string envVarName)
    {
        var encryptedBase64Text = Environment.GetEnvironmentVariable(envVarName);
        var encryptedBytes = Convert.FromBase64String(encryptedBase64Text);

        using (var client = new AmazonKeyManagementServiceClient())
        {
            var decryptRequest = new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(encryptedBytes),
            };

            var response = await client.DecryptAsync(decryptRequest);
            using (var plaintextStream = response.Plaintext)
            {
                var plaintextBytes = plaintextStream.ToArray();
                var plaintext = Encoding.UTF8.GetString(plaintextBytes);
                return plaintext;
            }
        }
    }
}
    }