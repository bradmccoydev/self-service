using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ProcessSlackSubmission.Entities;
using ProcessSlackSubmission.Entities.Block;
using ProcessSlackSubmission.Utilities;
using System.Data;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ProcessSlackSubmission
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
            StringHelper stringHelper = new StringHelper();
            CryptoHelper cryptoHelper = new CryptoHelper();
            Logger logger = new Logger();

            var environment = Environment.GetEnvironmentVariable("Environment");
            var slackAppCredentials = Environment.GetEnvironmentVariable("secret_id");
            var region = Environment.GetEnvironmentVariable("Region");
            var daxUrl = Environment.GetEnvironmentVariable("DaxUrl");
            int daxPort = Int32.Parse(Environment.GetEnvironmentVariable("DaxPort"));

            var secrets = await secretsManagerServiceAccessor
                .GetApplicationSecrets(
                    region: region,
                    secretId: slackAppCredentials);

            var date = DateTime.Now;
            var epochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var epoch = Convert.ToInt64((date - epochDateTime).TotalSeconds);


            if (json.ToString().Contains("31391478-e1f8-42ca-93b7-5ffac77c3292"))
            {
                return "Lambda Warmed";
            }

            var authorization = JsonConvert.DeserializeObject<Authorization.RootObject>(json.ToString());


            //var cleanedState = stringHelper.CleanState(authorization.body.state);

            //var blockState = JsonConvert.DeserializeObject<BlockState>("{" + cleanedState + "}");

            //if (blockState.self_service_environment == "staging")
            //{
            //    Console.WriteLine("*** Staging ***");
            //    signingSecret = signingSecretStaging;
            //    authToken = "xoxp-3643664388-309208909905-912080595424-4eed01ec4e3436eae56cefac231fca19";
            //    Console.WriteLine("freshy");
            //}
            //var slackTime = (long)Convert.ToDouble(authorization.headers.SlackRequestTimestamp);
            //var timeStampIsValid = utilities.ValidateTimeStamp(slackTime: slackTime);

            var signatureBaseString = $"v0:{authorization.headers.SlackRequestTimestamp}:{authorization.raw}";

            var computedSignature = cryptoHelper
                .HmacSha256Digest(
                    message: signatureBaseString,
                    secret: secrets.SigningSecret);

            if (authorization.headers.SlackSignature != "v0=" + computedSignature)
                //|| timeStampIsValid == false)
            {
                return "Api Usage Not Authorized";
            }

            if (json.ToString().Contains("\"type\": \"block_actions\""))
            {
                
                var blockSubmission = JsonConvert.DeserializeObject<BlockSubmission.RootObject>(json.ToString());

                var strippedId = stringHelper
                    .GetStringBeforeCharacter(
                        value: blockSubmission.body.container.message_ts,
                        character: ".");

                var submissionId = await dynamoDbDataAccessor
                    .GetSubmissionDataAsync(
                        accessKey: "",
                        secretKey: "",
                        sessionToken: "",
                        region: "us-west-2",
                        daxUrl: daxUrl,
                        daxPort: daxPort,
                        id: strippedId);

                    foreach (var action in blockSubmission.body.actions)
                    {
                    var cleanedState = stringHelper.CleanState(action.value);

                    var blockState = JsonConvert.DeserializeObject<BlockState>("{" + cleanedState + "}");

                    //if (blockState.self_service_environment == "staging")
                    //{
                    //    Console.WriteLine("*** Staging ***");
                    //    authToken = "xoxp-3643664388-309208909905-912080595424-4eed01ec4e3436eae56cefac231fca19";
                    //}


                    var channelName = await slackServiceAccessor
                        .GetChannelNameById(
                            token: secrets.AuthToken,
                            id: blockSubmission.body.channel.id,
                            user: blockSubmission.body.user.id,
                            channelType: blockSubmission.body.channel.name);

                        if (action.text.text == ":heavy_check_mark: Confirm")
                        {
                            if (blockState.Dialog <= blockState.number_of_dialogs)
                            {
                                if (blockState.Dialog == 2)
                                {
                                    var commandDetails = await dynamoDbDataAccessor
                                        .GetItemAsync(
                                            accessKey: "",
                                            secretKey: "",
                                            sessionToken: "",
                                            region: region,
                                            daxUrl: daxUrl,
                                            daxPort: daxPort,
                                            //tableName: $"command-{blockState.self_service_environment}",
                                            tableName: $"command-{environment.ToLower()}",
                                            id: blockState.callback_id,
                                            team: "T03JXKJBE");//send through when more teams

                                var userEmail = await slackServiceAccessor
                                    .GetUserEmail(
                                        token: secrets.BotUserToken,
                                        user: blockSubmission.body.user.id);

                                var userIsAuthorised = await dynamoDbDataAccessor
                                        .CheckIfUserIsAuthorised(
                                            accessKey: "",
                                            secretKey: "",
                                            sessionToken: "",
                                            region: region,
                                            daxUrl: daxUrl,
                                            daxPort: daxPort,
                                            userId: blockSubmission.body.user.id,
                                            userEmail: userEmail,
                                            authorisedUsers: commandDetails.AuthorisedUsers,
                                            authorisedGroups: commandDetails.AuthorisedGroups);

                                    //var response = await slackServiceAccessor
                                    //    .OpenDialog(
                                    //        token: authToken,
                                    //        dialogJson: form,
                                    //        triggerId: slashCommand.body.TriggerId);

                                    var form = "{\"callback_id\":\"" + blockState.callback_id
                                     + "\",\"title\":\"" + commandDetails.Title
                                     + "\",\"submit_label\": \"Request"
                                     + "\",\"state\":\"" + action.value
                                     + "\",\"elements\": [" + commandDetails.Dialog2
                                     + "]}";

                                    var response = await slackServiceAccessor
                                        .OpenDialog(
                                            token: secrets.AuthToken,
                                            dialogJson: form, //commandDetails.Dialog2.Replace("state\": \"\"", $"state\": \"{action.value}\""),
                                            triggerId: blockSubmission.body.trigger_id);
                                }
                            }
                        }

                        if (action.text.text == ":x: Cancel")
                        {
                            var response = await slackServiceAccessor
                                .DeleteSlackMessageAsync(
                                    token: secrets.AuthToken,
                                    channel: blockSubmission.body.channel.id,
                                    ts: blockSubmission.body.container.message_ts);
                        }

                        if (action.text.text == ":heavy_check_mark: Approve")
                        {
                            var request = await dynamoDbDataAccessor
                                .GetSubmissionDataAsync(
                                    accessKey: "",
                                    secretKey: "",
                                    sessionToken: "",
                                    region: region,
                                    daxUrl: daxUrl,
                                    daxPort: daxPort,
                                    id: submissionId.id);

                            if (request.endpoint.Contains("function"))
                            {
                                var lambdaResponse = lambdaServiceAccessor
                                     .InvokeLambdaFunction(
                                         accessKey: "",
                                         secretKey: "",
                                         sessionToken: "",
                                         region: region,
                                         functionName: "todo",
                                         payload: request.payload,
                                         requestResponse: "true");
                            }

                            if (request.endpoint.Contains("stateMachine"))
                            {
                                Console.WriteLine("Starting To Execute Step Function");

                            var stepFunctionPayload = "{" + $"\"title\":\" {request.id} \",\"step1\":" + $"{request.payload.TrimEnd('}')}, \"ResponseUrl\": \"{request.channel}\",\"SlackUser\":\"{request.user}\",\"TrackingId\":\"{blockSubmission.body.container.message_ts}\",\"Step\":\"0\",\"SlackChannel\": \"{request.channel}\"" + "}}";

                            var response = await stepFunctionServiceAccessor
                                    .StartExecutionAsync(
                                        accessKey: "",
                                        secretKey: "",
                                        sessionToken: "",
                                        region: region,
                                        requestInput: stepFunctionPayload,
                                        requestName: blockSubmission.body.container.message_ts,
                                        stateMachineArn: request.endpoint);
                        }

                                var deleteResponse = await slackServiceAccessor
                                    .DeleteSlackMessageAsync(
                                        token: secrets.AuthToken,
                                        channel: "GHUBBT7DH",
                                        ts: blockSubmission.body.container.message_ts);

                                await slackServiceAccessor
                                    .PostSlackMessageAsync(
                                        token: secrets.AuthToken,
                                        channel: "GHUBBT7DH",
                                        text: $"{blockSubmission.body.user.name} Approved a {request.command} request for {request.user} id: {request.id}");
                        }

                        if (action.text.text == ":x: Reject")
                        {
                            var request = await dynamoDbDataAccessor
                                .GetSubmissionDataAsync(
                                    accessKey: "",
                                    secretKey: "",
                                    sessionToken: "",
                                    region: region,
                                    daxUrl: daxUrl,
                                    daxPort: daxPort,
                                    id: submissionId.id);

                            var response = await slackServiceAccessor
                                .DeleteSlackMessageAsync(
                                    token: secrets.BotUserToken,
                                    channel: "GHUBBT7DH",
                                    ts: request.id);

                            await slackServiceAccessor
                                .PostSlackMessageAsync(
                                    token: secrets.BotUserToken,
                                    channel: "GHUBBT7DH",
                                    text: $"{blockSubmission.body.user.name} Rejected a {request.command} request for {request.user} id: {request.id}");

                            await slackServiceAccessor
                                .PostSlackDirectMessage(
                                    token: secrets.BotUserToken,
                                    user: blockSubmission.body.user.id,
                                    text: $"Im sorry your request was rejected by {blockSubmission.body.user.name}");
                        }
                    }  
            }

            if (json.ToString().Contains("\"type\": \"dialog_submission\""))
            {
                try
                {
                    var dialogSubmission = JsonConvert.DeserializeObject<DialogSubmission.RootObject>(json.ToString());

                    //var cleanedState = stringHelper.CleanState(dialogSubmission.body.state);

                    //var blockState = JsonConvert.DeserializeObject<BlockState>("{" + cleanedState + "}");

                    //if (blockState.self_service_environment == "staging")
                    //{
                    //    Console.WriteLine("*** Staging2 ***");
                    //    authToken = "xoxp-3643664388-309208909905-912080595424-4eed01ec4e3436eae56cefac231fca19";
                    //}

                    //Console.WriteLine($"Check This: {blockState.self_service_environment}");
                    //Console.WriteLine($"2: {dialogSubmission.body.callback_id}");
                    //Console.WriteLine($"Check This: {dialogSubmission.body.team.id}");

                    //var commandDetails = await dynamoDbDataAccessor
                    //    .GetItemAsync(
                    //        accessKey: "",
                    //        secretKey: "",
                    //        sessionToken: "",
                    //        region: region,
                    //        daxUrl: daxUrl,
                    //        daxPort: daxPort,
                    //        tableName: $"command-{blockState.self_service_environment}",
                    //        id: dialogSubmission.body.callback_id,
                    //        team: dialogSubmission.body.team.id);

                    var commandDetails = await dynamoDbDataAccessor
                       .GetItemAsync(
                           accessKey: "",
                           secretKey: "",
                           sessionToken: "",
                           region: region,
                           daxUrl: daxUrl,
                           daxPort: daxPort,
                           tableName: $"command-{environment.ToLower()}",
                           id: dialogSubmission.body.callback_id,
                           team: dialogSubmission.body.team.id);

                    var userEmail = await slackServiceAccessor
                        .GetUserEmail(
                            token: secrets.BotUserToken,
                            user: dialogSubmission.body.user.id);

                    var userIsAuthorised = await dynamoDbDataAccessor
                        .CheckIfUserIsAuthorised(
                            accessKey: "",
                            secretKey: "",
                            sessionToken: "",
                            region: region,
                            daxUrl: daxUrl,
                            daxPort: daxPort,
                            userId: dialogSubmission.body.user.id,
                            userEmail: userEmail,
                            authorisedUsers: commandDetails.AuthorisedUsers,
                            authorisedGroups: commandDetails.AuthorisedGroups);
                    
                    var payload = stringHelper
                        .GetStringBetweenTwoCharacters(
                            value: json.ToString(),
                            characterA: "\"submission\":",
                            characterB: "}");

                    var state = stringHelper
                        .AddStateToJsonPayload(
                            state: dialogSubmission.body.state,
                            json: payload);

                    var channelName = await slackServiceAccessor
                        .GetChannelNameById(
                            token: secrets.AuthToken,
                            id: dialogSubmission.body.channel.id,
                            user: dialogSubmission.body.user.id,
                            channelType: dialogSubmission.body.channel.name);

                    if (state.IsLastDialog == false)
                    {
                        var block = slackServiceAccessor
                            .BuildSlackMessageBlock(
                                title: commandDetails.Description,
                                callbackId: dialogSubmission.body.callback_id,
                                greenActionName: ":heavy_check_mark: Confirm",
                                redActionName: (channelName == "directmessage") ? "" : ":x: Cancel",//todo privategroup too
                                payload: state.Dictionary);

                        await slackServiceAccessor
                            .SendMessageBlockAsync(
                                token: secrets.AuthToken,
                                channel: channelName,
                                channelType: dialogSubmission.body.channel.name,
                                user: dialogSubmission.body.user.id,
                                block: block);
                    }

                    var approverMessage = new List<string>();

                    if (state.IsLastDialog == true)
                    {
                        payload = state.Payload.Replace("{", "").Replace("}", "");
                        
                        string stepFunctionPayload = "";

                        if (commandDetails.Endpoint.Contains("stateMachine"))
                        {
                            stepFunctionPayload = "{" + $"\"title\":\" {dialogSubmission.body.action_ts} \",\"step1\":" + "{" + $"{payload}, \"ResponseUrl\": \"{dialogSubmission.body.response_url}\",\"SlackUser\":\"{dialogSubmission.body.user.id}\",\"TrackingId\":\"{dialogSubmission.body.action_ts}\",\"Step\":\"0\",\"SlackChannel\": \"{dialogSubmission.body.channel.id}\"" + "}}";
                        }

                        if (commandDetails.LambdaName != null)
                        {
                            payload = payload.Replace(@"\n", "");
                            payload = payload.Replace("{\"", "{").Replace("\"}", "}");
                        }

                        if (commandDetails.RequiresApproval == "false")
                        {
                            if (commandDetails.LambdaName != null)
                            {
                                var lambdaResponse = lambdaServiceAccessor
                                    .InvokeLambdaFunction(
                                        accessKey: "",
                                        secretKey: "",
                                        sessionToken: "",
                                        region: region,
                                        functionName: commandDetails.LambdaName,
                                        payload: "{" + payload + "}",
                                        requestResponse: commandDetails.RequestResponse);
                            }

                            if (commandDetails.Endpoint.Contains("stateMachine"))
                            {
                                var response = await stepFunctionServiceAccessor
                                    .StartExecutionAsync(
                                        accessKey: "",
                                        secretKey: "",
                                        sessionToken: "",
                                        region: region,
                                        requestInput: stepFunctionPayload,
                                        requestName: dialogSubmission.body.action_ts,
                                        stateMachineArn: commandDetails.Endpoint);
                            }

                            if (dialogSubmission.body.channel.name == "privategroup")
                            {
                                await slackServiceAccessor
                                    .SendPostEphemeralAsync(
                                        token: secrets.BotUserToken,
                                        channel: channelName,
                                        user: dialogSubmission.body.user.id,
                                        text: $":smile: Thanks {dialogSubmission.body.user.name} Your request is being executed",
                                        attachments: "");
                            }
                            else
                            {
                                await slackServiceAccessor
                                    .SendMessageBlockAsync(
                                        token: secrets.BotUserToken,
                                        channel: channelName,
                                        channelType: dialogSubmission.body.channel.name,
                                        user: dialogSubmission.body.user.id,
                                        block: $":smile: Thanks {dialogSubmission.body.user.name} Your request is being executed");
                            }                                 
                        }

                        if (commandDetails.RequiresApproval == "true")
                        {
                            if (state.Dictionary.ContainsKey("user"))
                            {
                                state.Dictionary.Add("AvokaUser", dialogSubmission.body.user.name);
                            }

                            if (!state.Dictionary.ContainsKey("TrackingId"))
                            {
                                state.Dictionary.Add("TrackingId", dialogSubmission.body.action_ts);
                            }    

                            if (!state.Dictionary.ContainsKey("TrackingId"))
                            {
                                state.Dictionary.Add("Step", "0");
                            }                

                            var block = slackServiceAccessor
                                .BuildSlackMessageBlock(
                                    title: $"Approval: {commandDetails.Description}",
                                    callbackId: dialogSubmission.body.callback_id,
                                    greenActionName: ":heavy_check_mark: Approve",
                                    redActionName: ":x: Reject",
                                    payload: state.Dictionary);

                            var approvers = await dynamoDbDataAccessor
                                .GetApproverListAsync(
                                    accessKey: "",
                                    secretKey: "",
                                    sessionToken: "",
                                    region: region,
                                    daxUrl: daxUrl,
                                    daxPort: daxPort,
                                    tableName: "user-production",
                                    approverUsers: commandDetails.ApproverUsers,
                                    approverGroups: commandDetails.ApproverGroups);

                            var message_ts = await slackServiceAccessor
                                 .SendMessageBlockAsync(
                                     token: secrets.AuthToken,
                                     channel: "ops-approvals",
                                     channelType: dialogSubmission.body.channel.name,
                                     user: "",
                                     block: block);

                            approverMessage = approvers;
                            Console.WriteLine("message example" + message_ts);

                            await slackServiceAccessor
                                .SendMessageBlockAsync(
                                    token: secrets.AuthToken,
                                    channel: channelName,
                                    channelType: dialogSubmission.body.channel.name,
                                    user: dialogSubmission.body.user.id,
                                    block: $":smile: Thanks {dialogSubmission.body.user.name} Your request has been processed and is awaiting approval");
                        }

                        await logger.Log(
                            region: "us-west-2",
                            id: epoch.ToString(),
                            trackingId: dialogSubmission.body.action_ts,
                            command: dialogSubmission.body.callback_id,
                            user: dialogSubmission.body.user.name,
                            team: dialogSubmission.body.team.id,
                            channel: channelName,
                            payload: state.Payload,
                            endpoint: commandDetails.Endpoint,
                            approvers: approverMessage,
                            status: "Completed");
                    }
                }
                catch (Exception ex)
                {
                    var dialogSubmission = JsonConvert.DeserializeObject<DialogSubmission.RootObject>(json.ToString());

                    await logger.Log(
                            region: "us-west-2",
                            id: epoch.ToString(),
                            trackingId: dialogSubmission.body.action_ts,
                            command: dialogSubmission.body.callback_id,
                            user: dialogSubmission.body.user.name,
                            team: "T03JXKJBE",
                            channel: dialogSubmission.body.channel.name,
                            payload: $"{dialogSubmission.body.user.name} - {ex}",
                            endpoint: "",
                            approvers: new List<string>(),
                            status: "Error");
                }
            }

            return null;
        }
    }
}
