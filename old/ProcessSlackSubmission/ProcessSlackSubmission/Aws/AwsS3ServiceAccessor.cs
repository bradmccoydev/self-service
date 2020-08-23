using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using System.Text;

namespace ProcessSlackSubmission
{
    public class AwsS3ServiceAccessor
    {
        public async Task UploadFileToS3BucketFromByteArrayAsync(
            string bucketName,
            string folderName,
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string targetFileName,
            byte[] targetFileBytes,
            string contentType)
        {
            var s3client = await GetS3ClientAsync(
                accessKey: accessKey,
                secretKey: secretKey,
                sessionToken: sessionToken,
                region: region);

            using (var memoryStream = new MemoryStream())
            {
                MemoryStream myMemoryStream = new MemoryStream();
                Stream myStream = myMemoryStream;

                memoryStream.Write(targetFileBytes, 0, targetFileBytes.Length);
                memoryStream.Position = 0;

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    CannedACL = S3CannedACL.PublicRead,
                    BucketName = bucketName + folderName,
                    ContentType = contentType,
                    Key = targetFileName,
                    InputStream = memoryStream
                };

                var fileTransferUtility = new TransferUtility(s3client);
                fileTransferUtility.Upload(uploadRequest);

            }
        }

        public async Task<AmazonS3Client> GetS3ClientAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region)
        {
            AmazonS3Client s3Client = new AmazonS3Client();

            var iamRegion = RegionEndpoint.GetBySystemName(region);

            if (sessionToken != "")
            {
                SessionAWSCredentials awsCreds = new SessionAWSCredentials(
                    awsAccessKeyId: accessKey,
                    awsSecretAccessKey: secretKey,
                    token: sessionToken);

                s3Client = new AmazonS3Client(
                    awsCreds,
                    iamRegion);
            }
            if (sessionToken == ""
                && accessKey != "")
            {
                BasicAWSCredentials awsCreds = new BasicAWSCredentials(
                    accessKey: accessKey,
                    secretKey: secretKey);

                s3Client = new AmazonS3Client(
                    awsCreds, iamRegion);
            }
            if (accessKey == "")
            {
                s3Client = new AmazonS3Client(iamRegion);
            }

            return s3Client;

        }
    }
}
