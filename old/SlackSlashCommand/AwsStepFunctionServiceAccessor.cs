using Amazon;
using Amazon.Runtime;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using System.Threading;
using System.Threading.Tasks;

namespace SlackSlashCommand
{
    public class AwsStepFunctionServiceAccessor
    {
        public async Task CreateStateMachineAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string requestDefintion,
            string requestName,
            string requestRoleArn)
        {
            var stepFunctionClient = await GetStepFunctionClientAsync(
                accessKey: accessKey,
                secretKey: secretKey,
                sessionToken: sessionToken,
                region: region);

            CreateStateMachineRequest request = new CreateStateMachineRequest();
            request.Definition = requestDefintion;
            request.Name = requestName;
            request.RoleArn = requestRoleArn;

            CancellationToken cancellationToken = new CancellationToken();

            await stepFunctionClient.CreateStateMachineAsync(
                request: request,
                cancellationToken: cancellationToken);
        }

        public async Task<string> StartExecutionAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string requestInput,
            string requestName,
            string stateMachineArn)
        {
            var stepFunctionClient = await GetStepFunctionClientAsync(
                accessKey: accessKey,
                secretKey: secretKey,
                sessionToken: sessionToken,
                region: region);

            StartExecutionRequest request = new StartExecutionRequest();
            request.Input = requestInput;
            request.Name = requestName;
            request.StateMachineArn = stateMachineArn;

            CancellationToken cancellationToken = new CancellationToken();

            var response = await stepFunctionClient.StartExecutionAsync(
                request: request,
                cancellationToken: cancellationToken);

            return response.HttpStatusCode.ToString();
        }

        public async Task<AmazonStepFunctionsClient> GetStepFunctionClientAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region)
        {
            AmazonStepFunctionsClient stepFunctionClient = new AmazonStepFunctionsClient();

            var iamRegion = RegionEndpoint.GetBySystemName(region);

            if (sessionToken != "")
            {
                SessionAWSCredentials awsCreds = new SessionAWSCredentials(
                    awsAccessKeyId: accessKey,
                    awsSecretAccessKey: secretKey,
                    token: sessionToken);

                stepFunctionClient = new AmazonStepFunctionsClient(
                    awsCreds,
                    iamRegion);
            }
            else
            {
                BasicAWSCredentials awsCreds = new BasicAWSCredentials(
                    accessKey: accessKey,
                    secretKey: secretKey);

                stepFunctionClient = new AmazonStepFunctionsClient(
                    awsCreds, iamRegion);
            }

            return stepFunctionClient;

        }
    }
}
