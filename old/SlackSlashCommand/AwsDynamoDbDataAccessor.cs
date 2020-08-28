using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System.Collections.Generic;
using System;
using System.Threading;
using SlackSlashCommand.Entities; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Amazon.DAX;
using System.Linq;

namespace SlackSlashCommand
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

                    int groupAllowed = 0;

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

                    int groupAllowed = 0;

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

        public async Task<string> ScanTableAsync(
          string accessKey,
          string secretKey,
          string sessionToken,
          string region,
          string daxUrl,
          int daxPort,
          string tableName)
        {
            //List<dynamic> dynamicList = new List<dynamic>();
            List<Help> commandList = new List<Help>();

            if (daxUrl.Contains(".com"))
            {
                var client = GetDaxClient(
                    daxUrl: daxUrl,
                    daxPort: daxPort,
                    region: region);

                Table peopleTable = Table.LoadTable(client, tableName);
                ScanFilter scanFilter = new ScanFilter();
                Search getAllItems = peopleTable.Scan(scanFilter);
                var conditions = new List<ScanCondition>();
                List<Document> allItems = await getAllItems.GetRemainingAsync();

                foreach (Document item in allItems)
                {
                    var json = item.ToJson();
                    var dbDetail = JsonConvert.DeserializeObject<Help>(json);
                    // var dbDetail = JsonConvert.DeserializeObject<dynamic>(json);
                    commandList.Add(dbDetail);
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
                        var dbDetail = JsonConvert.DeserializeObject<Help>(json);
                        // var dbDetail = JsonConvert.DeserializeObject<dynamic>(json);
                        commandList.Add(dbDetail);
                    }
                }
            }

            return JsonConvert.SerializeObject(commandList);
        }

        public async Task<string> GetAvailableCommandsForUser(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string userEmail,
            string commandTable)
        {
            var commandString = "";

            var tableJson = await ScanTableAsync(
                accessKey: accessKey,
                secretKey: secretKey,
                sessionToken: sessionToken,
                region: region,
                daxUrl: daxUrl,
                daxPort: daxPort,
                tableName: commandTable);

            var commands = JsonConvert.DeserializeObject<List<Help>>(tableJson);

            var list = new List<string>();
            foreach (var command in commands)
            {
                if (command.AuthorisedUsers.Contains(userEmail)
                    || command.AuthorisedUsers.Contains("*"))
                {
                    list.Add($"{command.Command} - {command.Description}");
                }
            }

            list.Sort();

            foreach (var key in list)
            {
                commandString = commandString + $"{key} {Environment.NewLine}";
            }

            return commandString;
        }

        public async Task<CommandDetails> GetItemAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string tableName,
            string id)
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
                stringList.Add("dialog");
                stringList.Add("command");
                stringList.Add("title");
                stringList.Add("preState");
                stringList.Add("type");
                stringList.Add("endpoint");
                stringList.Add("authorised_users");
                stringList.Add("authorised_groups");
                stringList.Add("description");
                stringList.Add("block_action");

                request.AttributesToGet = stringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "command")
                    {
                        commandDetails.Command = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "title")
                    {
                        commandDetails.Title = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "preState")
                    {
                        commandDetails.PreState = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "block_action")
                    {
                        commandDetails.BlockAction = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "dialog")
                    {
                        commandDetails.Dialog = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "type")
                    {
                        commandDetails.Type = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "endpoint")
                    {
                        commandDetails.Endpoint = attribute.Value.S.ToString();
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

                    if (attribute.Key == "description")
                    {
                        commandDetails.Description = attribute.Value.S.ToString();
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
                stringList.Add("dialog");
                stringList.Add("command");
                stringList.Add("title");
                stringList.Add("preState");
                stringList.Add("type");
                stringList.Add("endpoint");
                stringList.Add("authorised_users");
                stringList.Add("authorised_groups");
                stringList.Add("description");
                stringList.Add("block_action");

                request.AttributesToGet = stringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "command")
                    {
                        commandDetails.Command = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "title")
                    {
                        commandDetails.Title = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "preState")
                    {
                        commandDetails.PreState = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "block_action")
                    {
                        commandDetails.BlockAction = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "dialog")
                    {
                        commandDetails.Dialog = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "type")
                    {
                        commandDetails.Type = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "endpoint")
                    {
                        commandDetails.Endpoint = attribute.Value.S.ToString();
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

                    if (attribute.Key == "description")
                    {
                        commandDetails.Description = attribute.Value.S.ToString();
                    }
                }
            }

           return commandDetails;
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
            StepFunctionDetails stepFunctionDetails = new StepFunctionDetails();

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
            }

            return stepFunctionDetails;
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
            if (sessionToken == "")
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
