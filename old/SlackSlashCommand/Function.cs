
using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using SlackSlashCommand.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SlackSlashCommand
{
    public class Function
    {
        public async Task<string> FunctionHandler(
            JObject json,
            ILambdaContext context)
        {
            SlackServiceAccessor slackServiceAccessor = new SlackServiceAccessor();
            AwsSecretsManagerServiceAccessor secretsManagerServiceAccessor = new AwsSecretsManagerServiceAccessor();
            AwsDynamoDbDataAccessor dynamoDbDataAccessor = new AwsDynamoDbDataAccessor();
            AwsLambdaServiceAccessor lambdaServiceAccessor = new AwsLambdaServiceAccessor();
            AwsStepFunctionServiceAccessor stepFunctionServiceAccessor = new AwsStepFunctionServiceAccessor();
            Utilities utilities = new Utilities();
            Logger logger = new Logger();

            Console.WriteLine(json.ToString());
            
            var environment = "DEV";
            var slackAppCredentials = "self-service";
            var region = "us-west-2";
            var daxUrl = "Environment.GetEnvironmentVariable()";
            int daxPort = 0;

            var secrets = await secretsManagerServiceAccessor
                .GetApplicationSecrets(
                    region: region,
                    secretId: slackAppCredentials);

            var x = Environment.NewLine;
            var guid = Guid.NewGuid().ToString();
            var output = "";

            var slashCommand = JsonConvert.DeserializeObject<SlashCommand.RootObject>(json.ToString());

            await slackServiceAccessor
                .SendSlackMessageAsync(
                    url: slashCommand.body.ResponseUrl,
                    token: "",
                    channel: "Brad McCoy",
                    message: json.ToString());

            var channelName = await slackServiceAccessor
                .GetChannelNameById(
                    token: secrets.AuthToken,
                    id: slashCommand.body.ChannelId,
                    user: slashCommand.body.UserId,
                    channelType: slashCommand.body.ChannelName);

            var userEmail = await slackServiceAccessor
                .GetUserEmail(
                    token: secrets.BotUserToken,
                    user: slashCommand.body.UserId);

            if (slashCommand.body.Text == "help")
            {
                var commands = await dynamoDbDataAccessor
                    .GetAvailableCommandsForUser(
                        accessKey: "",
                        secretKey: "",
                        sessionToken: "",
                        region: region,
                        daxUrl: daxUrl,
                        daxPort: daxPort,
                        userEmail: userEmail,
                        commandTable: $"command-{environment.ToLower()}");

                if (slashCommand.body.ChannelName == "privategroup")
                {
                    var blockTs = await slackServiceAccessor
                        .SendMessageBlockAsync(
                            token: secrets.AuthToken,
                            channel: channelName,
                            channelType: slashCommand.body.ChannelId,
                            user: slashCommand.body.UserId,
                            block: "[{\"type\": \"image\",\"title\": {\"type\": \"plain_text\",\"text\": \"ACS Self Service\",\"emoji\": true},\"image_url\": \"https://bradmccoy.s3.us-west-2.amazonaws.com/shellyacs.png?AWSAccessKeyId=AKIAWVIKMZQ6X5PPQ3XT&Expires=1597142877&Signature=zMmyO0gkh4u0JCI3UWib9Js9wSk%3D\",\"alt_text\": \"Self Service\"}]");
                            
                    var messageResponse = await slackServiceAccessor
                        .SendPostEphemeralAsync(
                            token: secrets.AuthToken,
                            channel: channelName,
                            user: slashCommand.body.UserId,
                            text: commands,
                            message: "Here are the Commands"); 
                }
                else
                {
                    return ":shelly: Wrong channel Please use temenos-self-service :shelly:";
                }

                await logger.Log(
                    region: region,
                    id: guid,
                    trackingId: slashCommand.body.TriggerId,
                    command: slashCommand.body.Text,
                    user: slashCommand.body.UserName,
                    team: slashCommand.body.TeamId,
                    channel: channelName,
                    payload: "Help Command",
                    endpoint: "",
                    approvers: null,
                    status: "Help");

                return $"Please see above your available commands, for more information go here: https://support.avoka.com/confluence/display/APD/Slack+Self-Service"; 
            }

            var slackTime = (long)Convert.ToDouble(slashCommand.headers.SlackRequestTimestamp);
            var timeStampIsValid = true;//utilities.ValidateTimeStamp(slackTime: slackTime);

            var payload = utilities
                .JsonToUrlString(json: JsonConvert.SerializeObject(slashCommand.body));
                    
            var signatureBaseString = $"v0:{slashCommand.headers.SlackRequestTimestamp}:{payload}";

            var computedSignature = "v0=" +
                utilities
                   .HmacSha256Digest(
                       message: signatureBaseString,
                       secret: secrets.SigningSecret);

            if (slashCommand.headers.SlackSignature != computedSignature
                || timeStampIsValid == false)
            {
                Console.WriteLine("xx");
                return "Api Usage Not Authorized";
            }

            Console.WriteLine("hellllo");

            var commandDetails = await dynamoDbDataAccessor
                .GetItemAsync(
                    accessKey: "",
                    secretKey: "",
                    sessionToken: "",
                    region: region,
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    tableName: $"command-{environment.ToLower()}",
                    id: slashCommand.body.Text);

            if (commandDetails.Command == null)
            {
                return ":thinking_face: Sorry I dont know this command please use /shelly help";
            }

            Console.WriteLine($"users: {commandDetails.AuthorisedUsers}");

            var userIsAuthorised = await dynamoDbDataAccessor
                .CheckIfUserIsAuthorised(
                    accessKey: "",
                    secretKey: "",
                    sessionToken: "",
                    region: region,
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    userId: slashCommand.body.UserId,
                    userEmail: userEmail,
                    authorisedUsers: commandDetails.AuthorisedUsers,
                    authorisedGroups: commandDetails.AuthorisedGroups);  

            if (userIsAuthorised != null)
            {
                try
                {
                    if (commandDetails.Type == "dialog_submission")
                    {
                        var form = "{\"callback_id\":\"" + commandDetails.Command
                            + "\",\"title\":\"" + commandDetails.Title
                            + "\",\"submit_label\": \"Request"
                            + "\",\"state\":\"'dialog':'1'," + commandDetails.PreState
                            + "\",\"elements\": [" + commandDetails.Dialog
                            + "]}";

                        var response = await slackServiceAccessor
                            .OpenDialog(
                                token: secrets.AuthToken,
                                dialogJson: form,
                                triggerId: slashCommand.body.TriggerId);

                        var warmResponse = await lambdaServiceAccessor
                           .InvokeLambdaFunction(
                               accessKey: "",
                               secretKey: "",
                               sessionToken: "",
                               region: region,
                               functionName: "ProcessSlackSubmission",
                               payload: "{\"warm\":\"31391478-e1f8-42ca-93b7-5ffac77c3292\"}",
                               requestResponse: "false");

                        await logger.Log(
                            region: region,
                            id: guid,
                            trackingId: slashCommand.body.TriggerId,
                            command: commandDetails.Command,
                            user: slashCommand.body.UserName,
                            team: slashCommand.body.TeamId,
                            channel: channelName,
                            payload: $"{commandDetails.Command} Requested",
                            endpoint: (commandDetails.Endpoint == null) ? "" : commandDetails.Endpoint,
                            approvers: new List<string>(),
                            status: "Requested");
                    }

                    if (commandDetails.Endpoint.Contains("function")
                        && commandDetails.Type != "dialog_submission")
                    {
                        var lambdaPayload = new LambdaPayload();
                        lambdaPayload.ResponseUrl = slashCommand.body.ResponseUrl;
                        lambdaPayload.SlackChannel = slashCommand.body.ChannelName;
                        lambdaPayload.SlackUser = slashCommand.body.UserId;
                        lambdaPayload.TrackingId = slashCommand.body.TriggerId;

                        string payloadLambda = JsonConvert.SerializeObject(lambdaPayload);

                        var lambdaName = utilities
                            .GetStringAfterCharacter(
                                value: commandDetails.Endpoint,
                                character: "function:");

                        var response = await lambdaServiceAccessor
                            .InvokeLambdaFunction(
                                accessKey: "",
                                secretKey: "",
                                sessionToken: "",
                                region: region,
                                functionName: lambdaName,
                                payload: payloadLambda,
                                requestResponse: "false");

                        output = $"Lambda Response: {response}";     
                    }

                    if (commandDetails.Endpoint.Contains("stateMachine")
                        && commandDetails.Type != "dialog_submission")
                    {
                        string requestName = Guid.NewGuid().ToString();

                        var stepMachineConfig = await dynamoDbDataAccessor
                            .GetStepFunctionDetailsAsync(
                                accessKey: "",
                                secretKey: "",
                                sessionToken: "",
                                region: region,
                                daxUrl: daxUrl,
                                daxPort: daxPort,
                                tableName: "job",
                                id: commandDetails.JobId);

                        var response = await stepFunctionServiceAccessor
                            .StartExecutionAsync(
                                accessKey: "",
                                secretKey: "",
                                sessionToken: "",
                                region: region,
                                requestInput: stepMachineConfig.StateMachineConfig,
                                requestName: requestName,
                                stateMachineArn: stepMachineConfig.StateMachineArn);

                        output = $"Step Function Response: {response}";
                    }

                    if (commandDetails.Type == "block_action")
                    {
                        // var block = slackServiceAccessor
                        //     .BuildSlackMessageBlock(
                        //         title: commandDetails.Description,
                        //         callbackId: dialogSubmission.body.callback_id,
                        //         greenActionName: ":heavy_check_mark: Confirm",
                        //         redActionName: ":x: Cancel",
                        //         payload: state.Dictionary);

                        var blockTs = await slackServiceAccessor
                           .SendMessageBlockAsync(
                               token: secrets.AuthToken,
                               channel: channelName,
                               channelType: slashCommand.body.ChannelName,
                               user: slashCommand.body.UserId,
                               block: commandDetails.BlockAction);

                        await logger.Log(
                            region: region,
                            id: blockTs,
                            trackingId: slashCommand.body.TriggerId,
                            command: commandDetails.Command,
                            user: slashCommand.body.UserName,
                            team: slashCommand.body.TeamId,
                            channel: channelName,
                            payload: commandDetails.BlockAction,
                            endpoint: "arn:aws:states:us-west-2:457972698173:stateMachine:Wfh",
                            approvers: new List<string>(),
                            status: "BlockSent");
                    }
                }
                catch (Exception ex)
                {
                    if (slashCommand.body.ChannelName == "directmessage")
                    {
                        await slackServiceAccessor
                            .SendCodeSnippetAsync(
                                token: secrets.BotUserToken,
                                channel: channelName,
                                title: "",
                                snippet: $"Your command didnt work: SlashCommand:{slashCommand.body.Command}-Response Url: {slashCommand.body.ResponseUrl} {x} Text: {slashCommand.body.Text} {x} User Id: {slashCommand.body.UserId} {x} UserName: {slashCommand.body.UserName} {x} TriggerId: {slashCommand.body.TriggerId} Ex - {ex}",
                                user: "");
                    }

                    await slackServiceAccessor
                        .SendSlackMessageAsync(
                            url: slashCommand.body.ResponseUrl,
                            token: "",
                            channel: "avoka-cloud-logs",
                            message: $"SlashCommand:{slashCommand.body.Command}-Response Url: {slashCommand.body.ResponseUrl} {x} Text: {slashCommand.body.Text} {x} User Id: {slashCommand.body.UserId} {x} UserName: {slashCommand.body.UserName} {x} TriggerId: {slashCommand.body.TriggerId} Ex - {ex}");

                    await logger.Log(
                            region: region,
                            id: guid,
                            trackingId: slashCommand.body.TriggerId,
                            command: commandDetails.Command,
                            user: slashCommand.body.UserName,
                            team: slashCommand.body.TeamId,
                            channel: channelName,
                            payload: $"{slashCommand.body.UserName} - {ex}",
                            endpoint: "",
                            approvers: new List<string>(),
                            status: "Error");


                }
            }
            else
            {
                output = (slashCommand == null) ? "Command Empty" : output;
                output = (userIsAuthorised == null) ? $"User Not Authorised {slashCommand.body.UserId}" : output;
            }

            return $"Thanks for executing the slash command: {slashCommand.body.Text}{output}";
        }
    }
 }

