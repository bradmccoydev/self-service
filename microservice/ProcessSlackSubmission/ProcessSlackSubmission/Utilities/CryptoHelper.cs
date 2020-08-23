using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace ProcessSlackSubmission.Utilities
{
    public class CryptoHelper
    {
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

        public string HmacSha256Digest(
            string message,
            string secret)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            HMACSHA256 cryptographer = new HMACSHA256(keyBytes);

            byte[] bytes = cryptographer.ComputeHash(messageBytes);

            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public bool ValidateTimeStamp(long slackTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            bool withinTimeRange = false;
            var difference = unixTimestamp - slackTime;

            if (difference > 0
                && difference < 20)
            {
                withinTimeRange = true;
            }

            return withinTimeRange;
        }
    }
}
