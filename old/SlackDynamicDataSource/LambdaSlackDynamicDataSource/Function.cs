using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSlackDynamicDataSource.Entities;
using LambdaSlackDynamicDataSource.Authorization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LambdaSlackDynamicDataSource.DataAccessors;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaSlackDynamicDataSource
{
    public class Function
    {
        public async Task<string> FunctionHandler(
            JObject json,
            ILambdaContext context)
        {
            SlackServiceAccessor slackServiceAccessor = new SlackServiceAccessor();
            AwsKmsServiceAccessor kmsServiceAccessor = new AwsKmsServiceAccessor();
            AwsDynamoDbDataAccessor dynamoDbDataAccessor = new AwsDynamoDbDataAccessor();
            Utilities utilities = new Utilities();

            var daxUrl = Environment.GetEnvironmentVariable("DaxUrl");
            int daxPort = Int32.Parse(Environment.GetEnvironmentVariable("DaxPort"));
            var region = Environment.GetEnvironmentVariable("Region");

            var signingSecret = await kmsServiceAccessor
                .DecodeEnvVar("SigningSecret");

            var authToken = await kmsServiceAccessor
                .DecodeEnvVar("AuthToken");

            if (json.ToString().Contains("31391478-e1f8-42ca-93b7-5ffac77c3292"))
            {
                Console.WriteLine("Warmed");
                return null;
            }

            var input = JsonConvert.DeserializeObject<Input.RootObject>(json.ToString());

            var output = "{'options':[{'label':'Error - Contact ACS','value':'error'}]}";

            try
            {
               // var slackTime = (long)Convert.ToDouble(input.headers.SlackRequestTimestamp);
                var signatureBaseString = $"v0:{input.headers.SlackRequestTimestamp}:{input.body}";

                var computedSignature = "v0=" +
                    utilities
                       .HmacSha256Digest(
                           message: signatureBaseString,
                           secret: signingSecret);

                //var timeStampIsValid = utilities.ValidateTimeStamp(slackTime: slackTime);

                if (input.headers.SlackSignature != computedSignature)
                    //|| timeStampIsValid == false)
                {
                    return "Api Usage Not Authorized";
                }

                var decodedPayload = utilities
                        .UrlDecodeString(value: input.body);

                var dialogSuggestion = JsonConvert.DeserializeObject<DialogSuggestion.RootObject>(decodedPayload.Replace("payload=",""));

                var commandDetails = await dynamoDbDataAccessor
                    .GetCommandDetailsAsync(
                        accessKey: "",
                        secretKey: "",
                        sessionToken: "",
                        region: region,
                        daxUrl: daxUrl,
                        daxPort: daxPort,
                        tableName: "command-production",
                        partitionKey: dialogSuggestion.callback_id,
                        sortKey: dialogSuggestion.team.id);

                //var userIsAuthorised = await dynamoDbDataAccessor
                //    .CheckIfUserIsAuthorised(
                //        accessKey: "",
                //        secretKey: "",
                //        sessionToken: "",
                //        region: region,
                //        daxUrl: daxUrl,
                //        daxPort: daxPort,
                //        userId: dialogSuggestion.user.id,
                //        authorisedUsers: commandDetails.AuthorisedUsers,
                //        authorisedGroups: commandDetails.AuthorisedGroups);

                var externalDataSource = JsonConvert.DeserializeObject<ExternalDataSource.RootObject>(commandDetails.ExternalDataSource);

                string tableName = "";
                string key = "";
                string value = "";
                string attribute = "";
                string source = "";
                var list = new List<string>();

                foreach (var dataSource in externalDataSource.values)
                {
                    if (dialogSuggestion.name == dataSource.name)
                    {
                        source = dataSource.source;

                        value = utilities
                            .GetStateValue(
                                state: dialogSuggestion.state,
                                name: dataSource.value);

                        tableName = dataSource.table;
                        key = dataSource.key;
                        attribute = dataSource.attribute;
                    }
                }

                if (source == "Lambda")//todo check.
                {
                    list = await dynamoDbDataAccessor
                        .GetStringListQueryAsync(
                            accessKey: "",
                            secretKey: "",
                            sessionToken: "",
                            region: region,
                            daxUrl: daxUrl,
                            daxPort: daxPort,
                            indexName: "",
                            tableName: tableName,
                            key: key,
                            value: value,
                            attribute: attribute);
                }

                if (source == "DynamoDb")//todo check.
                {

                    list = await dynamoDbDataAccessor
                        .GetStringListQueryAsync(
                            accessKey: "",
                            secretKey: "",
                            sessionToken: "",
                            region: region,
                            daxUrl: daxUrl,
                            daxPort: daxPort,
                            indexName: "",
                            tableName: tableName,
                            key: key,
                            value: value,
                            attribute: attribute);
                }

                var filteredList = new List<string>();

                foreach (var rawLine in list)
                {
                    var newline = utilities
                        .GetStringBetweenTwoCharacters(
                            value: rawLine,
                            characterA: "url=",
                            characterB: " ",
                            characterAIndex: "",
                            characterbIndex: "");

                    newline = rawLine.Replace($"url={newline}","");

                    filteredList.Add(newline);
                }

                output = slackServiceAccessor
                    .ConvertStringListToSlackObject(list: filteredList);

                // await slackServiceAccessor
                //    .SendSlackMessageAsync(
                //        url: "https://hooks.slack.com/services/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",
                //        token: authToken,
                //        channel: "avoka-cloud-logs",
                //        message: output);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dynamic Data error: {ex.ToString()}");
            }

            return output;
        }
    }
}
