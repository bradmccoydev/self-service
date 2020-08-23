using System;
using Amazon;
using Amazon.Runtime;
using Newtonsoft.Json;
using System.Threading;
using Amazon.DynamoDBv2;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ProcessSlackSubmission.Entities;
using Amazon.DAX;

namespace ProcessSlackSubmission
{
    public class AwsDynamoDbDataAccessor
    {
        public async Task<string> GetItemAsJsonAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string tableName,
            string id)
        {
            if (daxUrl.Contains(".com"))
            {
                var client = GetDaxClient(
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    region: region);

                Table table = Table.LoadTable(client, tableName);
                Document document = await table.GetItemAsync(id);
                return document.ToJson();
            }
            else
            {
                var client = await GetDynamoDbClient(
                    accessKey: accessKey,
                    secretKey: secretKey,
                    sessionToken: sessionToken,
                    region: region);

                Table table = Table.LoadTable(client, tableName);
                Document document = await table.GetItemAsync(id);
                return document.ToJson();
            }
        }

        public async Task<string> ScanTableAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string tableName)
        {
            List<dynamic> dynamicList = new List<dynamic>();

            try
            {
                if (daxUrl.Contains(".com"))
                {
                    using (var client = GetDaxClient(
                        daxUrl: daxUrl,
                        daxPort: daxPort,
                        region: region))
                    {
                        Table peopleTable = Table.LoadTable(client, tableName);
                        ScanFilter scanFilter = new ScanFilter();
                        Search getAllItems = peopleTable.Scan(scanFilter);
                        var conditions = new List<ScanCondition>();
                        List<Document> allItems = await getAllItems.GetRemainingAsync();

                        foreach (Document item in allItems)
                        {
                            var json = item.ToJson();
                            var dbDetail = JsonConvert.DeserializeObject<dynamic>(json);
                            dynamicList.Add(dbDetail);
                        }
                    }
                }
                else
                {
                    using (var client = await GetDynamoDbClient(
                        accessKey: accessKey,
                        secretKey: secretKey,
                        sessionToken: sessionToken,
                        region: region))
                    {
                        Table peopleTable = Table.LoadTable(client, tableName);
                        ScanFilter scanFilter = new ScanFilter();
                        Search getAllItems = peopleTable.Scan(scanFilter);
                        var conditions = new List<ScanCondition>();
                        List<Document> allItems = await getAllItems.GetRemainingAsync();

                        foreach (Document item in allItems)
                        {
                            var json = item.ToJson();
                            var dbDetail = JsonConvert.DeserializeObject<dynamic>(json);
                            dynamicList.Add(dbDetail);
                        }
                    }
                }
            }
            catch (AmazonDynamoDBException exception)
            {
                Console.WriteLine(string.Concat("Exception while filtering records in DynamoDb table: {0}", exception.Message));
                Console.WriteLine(String.Concat("Error code: {0}, error type: {1}", exception.ErrorCode, exception.ErrorType));
            }

            return JsonConvert.SerializeObject(dynamicList);
        }

        public async Task<List<string>> GetApproverListAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string tableName,
            List<string> approverUsers,
            List<string> approverGroups)
        {
            var approverlist = new List<string>();

            foreach (var approver in approverUsers)
            {
                approverlist.Add(approver);
            }

            var slackUserJson = await ScanTableAsync(
                accessKey: accessKey,
                secretKey: secretKey,
                sessionToken: sessionToken,
                region: region,
                daxUrl: daxUrl,
                daxPort: daxPort,
                tableName: tableName);

            var slackUser = JsonConvert.DeserializeObject<List<Approvers>>(slackUserJson);

            foreach (var user in slackUser)
            {
                if (user.groups != null)
                {
                    foreach (var group in user.groups)
                    {
                        if (approverGroups.Contains(group))
                        {
                            if (!approverlist.Contains(user.id))
                            {
                                approverlist.Add(user.id);
                            }
                        }
                    }
                }
            }

            return approverlist;
        }

        public async Task<string> CheckIfUserIsAuthorised(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string userId,
            string userEmail,
            string authorisedUsers,
            string authorisedGroups)
        {
            var userIsAuthorised = (authorisedUsers.Contains(userEmail) == false)
                ? null
                : userEmail;

            if (userIsAuthorised == null)
            {
                int groupAllowed = 0;

                if (daxUrl.Contains(".com"))
                {
                    var client = GetDaxClient(
                        daxUrl: daxUrl,
                        daxPort: daxPort,
                        region: region);

                    GetItemRequest request = new GetItemRequest();
                    request.TableName = "user-production";
                    request.Key = new Dictionary<string, AttributeValue>
                    {
                        {"id", new AttributeValue {S = userId}},
                        {"team", new AttributeValue {S = "T03JXKJBE"}}
                    };

                    List<String> stringList = new List<string>();
                    stringList.Add("groups");

                    request.AttributesToGet = stringList;

                    var item = await client.GetItemAsync(
                        request: request,
                        cancellationToken: new CancellationToken());

                    List<string> usersGroups = new List<string>();

                    foreach (var attribute in item.Item)
                    {
                        if (attribute.Key == "groups")
                        {
                            usersGroups = attribute.Value.SS;
                        }
                    }

                    foreach (var group in usersGroups)
                    {
                        groupAllowed = (authorisedGroups.Contains(group) == false)
                            ? groupAllowed
                            : groupAllowed + 1;
                    }

                    if (groupAllowed > 0)
                    {
                        userIsAuthorised = "yes";
                    }

                    if (authorisedUsers.Contains("*"))
                    {
                        userIsAuthorised = "yes";
                    }

                }
                else
                {
                    var client = await GetDynamoDbClient(
                        accessKey: accessKey,
                        secretKey: secretKey,
                        sessionToken: sessionToken,
                        region: region);

                    GetItemRequest request = new GetItemRequest();
                    request.TableName = "user-production";
                    request.Key = new Dictionary<string, AttributeValue>
                    {
                        {"id", new AttributeValue {S = userId}},
                        {"team", new AttributeValue {S = "T03JXKJBE"}}
                    };

                    List<String> stringList = new List<string>();
                    stringList.Add("groups");

                    request.AttributesToGet = stringList;

                    var item = await client.GetItemAsync(
                        request: request,
                        cancellationToken: new CancellationToken());

                    List<string> usersGroups = new List<string>();

                    foreach (var attribute in item.Item)
                    {
                        if (attribute.Key == "groups")
                        {
                            usersGroups = attribute.Value.SS;
                        }
                    }

                    foreach (var group in usersGroups)
                    {
                        groupAllowed = (authorisedGroups.Contains(group) == false)
                            ? groupAllowed
                            : groupAllowed + 1;
                    }

                    if (groupAllowed > 0)
                    {
                        userIsAuthorised = "yes";
                    }

                    if (authorisedUsers.Contains("*"))
                    {
                        userIsAuthorised = "yes";
                    }
                }
            }

            return userIsAuthorised;
        }

        public async Task<CommandDetails> GetItemAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string tableName,
            string id,
            string team)
        {
            CommandDetails commandDetails = new CommandDetails();

            if (daxUrl.Contains(".com"))
            {
                var client = GetDaxClient(
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    region: region);

                GetItemRequest request = new GetItemRequest();
                request.TableName = tableName;
                request.Key = new Dictionary<string, AttributeValue>
                {
                    {"command", new AttributeValue {S = id}},
                    {"team", new AttributeValue {S = "T03JXKJBE"}}
                };

                List<String> stringList = new List<string>();
                stringList.Add("description");
                stringList.Add("title");
                stringList.Add("dialog");
                stringList.Add("dialog2");
                stringList.Add("type");
                stringList.Add("authorised_users");
                stringList.Add("authorised_groups");
                stringList.Add("requires_authorization");
                stringList.Add("requires_approval");
                stringList.Add("approver_users");
                stringList.Add("approver_groups");
                stringList.Add("lambda_name");
                stringList.Add("endpoint");
                stringList.Add("request_response");

                request.AttributesToGet = stringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "description")
                    {
                        commandDetails.Description = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "title")
                    {
                        commandDetails.Title = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "dialog")
                    {
                        commandDetails.Dialog = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "dialog2")
                    {
                        commandDetails.Dialog2 = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "type")
                    {
                        commandDetails.Type = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "requires_authorization")
                    {
                        commandDetails.RequiresAuthorization = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "requires_approval")
                    {
                        commandDetails.RequiresApproval = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "lambda_name")
                    {
                        commandDetails.LambdaName = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "authorised_users")
                    {
                        commandDetails.AuthorisedUsers = string.Join(", ", attribute.Value.SS ?? new List<string>());
                        var list = "";
                    }

                    if (attribute.Key == "authorised_groups")
                    {
                        commandDetails.AuthorisedGroups = string.Join(", ", attribute.Value.SS ?? new List<string>());
                        var list = "";
                    }

                    if (attribute.Key == "approver_users")
                    {
                        commandDetails.ApproverUsers = attribute.Value.SS;
                    }

                    if (attribute.Key == "approver_groups")
                    {
                        commandDetails.ApproverGroups = attribute.Value.SS;
                    }

                    if (attribute.Key == "endpoint")
                    {
                        commandDetails.Endpoint = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "request_response")
                    {
                        commandDetails.RequestResponse = attribute.Value.S.ToString();
                    }
                }
            }
            else
            {
                var client = await GetDynamoDbClient(
                    accessKey: accessKey,
                    secretKey: secretKey,
                    sessionToken: sessionToken,
                    region: region);

                GetItemRequest request = new GetItemRequest();
                request.TableName = tableName;
                request.Key = new Dictionary<string, AttributeValue>
                {
                    {"command", new AttributeValue {S = id}},
                    {"team", new AttributeValue {S = "T03JXKJBE"}}
                };

                List<String> stringList = new List<string>();
                stringList.Add("description");
                stringList.Add("title");
                stringList.Add("dialog");
                stringList.Add("dialog2");
                stringList.Add("type");
                stringList.Add("authorised_users");
                stringList.Add("authorised_groups");
                stringList.Add("requires_authorization");
                stringList.Add("requires_approval");
                stringList.Add("approver_users");
                stringList.Add("approver_groups");
                stringList.Add("lambda_name");
                stringList.Add("endpoint");
                stringList.Add("request_response");

                request.AttributesToGet = stringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "description")
                    {
                        commandDetails.Description = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "title")
                    {
                        commandDetails.Title = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "dialog")
                    {
                        commandDetails.Dialog = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "dialog2")
                    {
                        commandDetails.Dialog2 = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "type")
                    {
                        commandDetails.Type = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "requires_authorization")
                    {
                        commandDetails.RequiresAuthorization = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "requires_approval")
                    {
                        commandDetails.RequiresApproval = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "lambda_name")
                    {
                        commandDetails.LambdaName = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "authorised_users")
                    {
                        commandDetails.AuthorisedUsers = string.Join(", ", attribute.Value.SS ?? new List<string>());
                        var list = "";
                    }

                    if (attribute.Key == "authorised_groups")
                    {
                        commandDetails.AuthorisedGroups = string.Join(", ", attribute.Value.SS ?? new List<string>());
                        var list = "";
                    }

                    if (attribute.Key == "approver_users")
                    {
                        commandDetails.ApproverUsers = attribute.Value.SS;
                    }

                    if (attribute.Key == "approver_groups")
                    {
                        commandDetails.ApproverGroups = attribute.Value.SS;
                    }

                    if (attribute.Key == "endpoint")
                    {
                        commandDetails.Endpoint = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "request_response")
                    {
                        commandDetails.RequestResponse = attribute.Value.S.ToString();
                    }
                }
            }

            return commandDetails;
        }

        public async Task<Submission> GetSubmissionDataAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string id)
        {
            var submission = new Submission();

            if (daxUrl.Contains(".com"))
            {
                var client = GetDaxClient(
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    region: region);

                GetItemRequest request = new GetItemRequest();
                request.TableName = "submission-production";
                request.Key = new Dictionary<string, AttributeValue>
                    {
                        {"id", new AttributeValue {S = id}},
                        {"team", new AttributeValue {S = "T03JXKJBE"}}
                    };

                List<String> stringList = new List<string>();
                stringList.Add("id");
                stringList.Add("command");
                stringList.Add("channel");
                stringList.Add("user");
                stringList.Add("payload");
                stringList.Add("endpoint");

                request.AttributesToGet = stringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "id")
                    {
                        submission.id = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "command")
                    {
                        submission.command = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "channel")
                    {
                        submission.channel = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "user")
                    {
                        submission.user = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "endpoint")
                    {
                        submission.endpoint = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "payload")
                    {
                        submission.payload = attribute.Value.S.ToString();
                    }
                }
            }
            else
            {
                var client = await GetDynamoDbClient(
                    accessKey: accessKey,
                    secretKey: secretKey,
                    sessionToken: sessionToken,
                    region: region);

                GetItemRequest request = new GetItemRequest();
                request.TableName = "submission-production";
                request.Key = new Dictionary<string, AttributeValue>
                    {
                        {"id", new AttributeValue {S = id}},
                        {"team", new AttributeValue {S = "T03JXKJBE"}}
                    };

                List<String> stringList = new List<string>();
                stringList.Add("id");
                stringList.Add("command");
                stringList.Add("channel");
                stringList.Add("user");
                stringList.Add("payload");
                stringList.Add("endpoint");

                request.AttributesToGet = stringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "id")
                    {
                        submission.id = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "command")
                    {
                        submission.command = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "channel")
                    {
                        submission.channel = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "user")
                    {
                        submission.user = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "endpoint")
                    {
                        submission.endpoint = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "payload")
                    {
                        submission.payload = attribute.Value.S.ToString();
                    }
                }
            }

            return submission;
        }

        public async Task PutItemJsonAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string tableName,
            string json)
        {

            if (daxUrl.Contains(".com"))
            {
                var client = GetDaxClient(
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    region: region);

                var item = Document.FromJson(json);

                var table = Table.LoadTable(client, tableName);

                var response = await table.PutItemAsync(
                    doc: item,
                    cancellationToken: new CancellationToken());
            }
            else
            {
                var client = await GetDynamoDbClient(
                    accessKey: accessKey,
                    secretKey: secretKey,
                    sessionToken: sessionToken,
                    region: region);

                var item = Document.FromJson(json);

                var table = Table.LoadTable(client, tableName);

                var response = await table.PutItemAsync(
                    doc: item,
                    cancellationToken: new CancellationToken());
            }
        }

        public async Task UpdateItemAsync(
           string accessKey,
           string secretKey,
           string sessionToken,
           string region,
           string daxUrl,
           int daxPort,
           string tableName,
           string primaryKey,
           string exception)
        {
            if (daxUrl.Contains(".com"))
            {
                var client = GetDaxClient(
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    region: region);

                var request = new UpdateItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { S = primaryKey } } },
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        {"#G", "Exception"},
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        {":Exception",new AttributeValue {S = exception}},
                    },

                    UpdateExpression = "SET #G = :Exception"
                };

                var response = await client.UpdateItemAsync(request);
            }
            else
            {
                var client = await GetDynamoDbClient(
                    accessKey: accessKey,
                    secretKey: secretKey,
                    sessionToken: sessionToken,
                    region: region);

                var request = new UpdateItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { S = primaryKey } } },
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        {"#G", "Exception"},
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        {":Exception",new AttributeValue {S = exception}},
                    },

                    UpdateExpression = "SET #G = :Exception"
                };

                var response = await client.UpdateItemAsync(request);
            }
        }

        public async Task<StepFunctionDetails> GetStepFunctionDetailsAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string tableName,
            string id)
        {
            if (daxUrl.Contains(".com"))
            {
                var client = GetDaxClient(
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    region: region);

                GetItemRequest request = new GetItemRequest();
                request.TableName = tableName;
                request.Key = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue {S = id}}
                };

                List<String> stateMachineConfigStringList = new List<string>();
                stateMachineConfigStringList.Add("state_machine_config");
                stateMachineConfigStringList.Add("state_machine_arn");

                request.AttributesToGet = stateMachineConfigStringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                StepFunctionDetails stepFunctionDetails = new StepFunctionDetails();

                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "state_machine_config")
                    {
                        stepFunctionDetails.StateMachineConfig = attribute.Value.S.ToString().Replace("\n", "").Replace("   ", "");
                    }

                    if (attribute.Key == "state_machine_arn")
                    {
                        stepFunctionDetails.StateMachineArn = attribute.Value.S.ToString();
                    }
                }

                return stepFunctionDetails;
            }
            else
            {
                var client = await GetDynamoDbClient(
                    accessKey: accessKey,
                    secretKey: secretKey,
                    sessionToken: sessionToken,
                    region: region);

                GetItemRequest request = new GetItemRequest();
                request.TableName = tableName;
                request.Key = new Dictionary<string, AttributeValue>
                {
                    {"id", new AttributeValue {S = id}}
                };

                List<String> stateMachineConfigStringList = new List<string>();
                stateMachineConfigStringList.Add("state_machine_config");
                stateMachineConfigStringList.Add("state_machine_arn");

                request.AttributesToGet = stateMachineConfigStringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                StepFunctionDetails stepFunctionDetails = new StepFunctionDetails();

                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "state_machine_config")
                    {
                        stepFunctionDetails.StateMachineConfig = attribute.Value.S.ToString().Replace("\n", "").Replace("   ", "");
                    }

                    if (attribute.Key == "state_machine_arn")
                    {
                        stepFunctionDetails.StateMachineArn = attribute.Value.S.ToString();
                    }
                }

                return stepFunctionDetails;
            }
        }

        public async Task<AmazonDynamoDBClient> GetDynamoDbClient(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region)
        {
            AmazonDynamoDBClient dynamoDBClient = new AmazonDynamoDBClient();

            var iamRegion = RegionEndpoint.GetBySystemName(region);

            if (sessionToken != "")
            {
                SessionAWSCredentials awsCreds = new SessionAWSCredentials(
                    awsAccessKeyId: accessKey,
                    awsSecretAccessKey: secretKey,
                    token: sessionToken);

                dynamoDBClient = new AmazonDynamoDBClient(
                    awsCreds,
                    iamRegion);
            }
            if (sessionToken == ""
                && accessKey != "")
            {
                BasicAWSCredentials awsCreds = new BasicAWSCredentials(
                    accessKey: accessKey,
                    secretKey: secretKey);

                dynamoDBClient = new AmazonDynamoDBClient(
                    awsCreds, iamRegion);
            }
            if (accessKey == "")
            {
                dynamoDBClient = new AmazonDynamoDBClient(iamRegion);
            }

            return dynamoDBClient;

        }

        public ClusterDaxClient GetDaxClient(
            string daxUrl,
            int daxPort,
            string region)
        {
            var clientConfig = new DaxClientConfig(daxUrl, daxPort)
            {
                AwsCredentials = FallbackCredentialsFactory.GetCredentials()
            };

            clientConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(region);
            var client = new ClusterDaxClient(clientConfig);

            return client;
        }
    }
}
