using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DAX;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using LambdaSlackDynamicDataSource.Entities;
using Newtonsoft.Json;

namespace LambdaSlackDynamicDataSource.DataAccessors
{
    public class AwsDynamoDbDataAccessor
    {
        public async Task<CommandDetails> GetCommandDetailsAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string tableName,
            string partitionKey,
            string sortKey)
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
                    {"command", new AttributeValue {S = partitionKey}},
                    {"team", new AttributeValue {S = sortKey}}
                };

                List<String> stringList = new List<string>();
                stringList.Add("authorised_users");
                stringList.Add("external_data_source");
                stringList.Add("authorised_groups");

                request.AttributesToGet = stringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());


                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "external_data_source")
                    {
                        commandDetails.ExternalDataSource = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "authorised_users")
                    {
                        commandDetails.AuthorisedUsers = string.Join(", ", attribute.Value.SS ?? new List<string>());
                    }

                    if (attribute.Key == "authorised_groups")
                    {
                        commandDetails.AuthorisedGroups = string.Join(", ", attribute.Value.SS ?? new List<string>());
                    }
                }
            }
            else
            {
                var client = GetDynamoDbClient(
                    accessKey: accessKey,
                    secretKey: secretKey,
                    sessionToken: sessionToken,
                    region: region);

                GetItemRequest request = new GetItemRequest();
                request.TableName = tableName;
                request.Key = new Dictionary<string, AttributeValue>
                {
                    {"command", new AttributeValue {S = partitionKey}},
                    {"team", new AttributeValue {S = sortKey}}
                };

                List<String> stringList = new List<string>();
                stringList.Add("authorised_users");
                stringList.Add("external_data_source");
                stringList.Add("authorised_groups");

                request.AttributesToGet = stringList;

                var item = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());


                foreach (var attribute in item.Item)
                {
                    if (attribute.Key == "external_data_source")
                    {
                        commandDetails.ExternalDataSource = attribute.Value.S.ToString();
                    }

                    if (attribute.Key == "authorised_users")
                    {
                        commandDetails.AuthorisedUsers = string.Join(", ", attribute.Value.SS ?? new List<string>());
                    }

                    if (attribute.Key == "authorised_groups")
                    {
                        commandDetails.AuthorisedGroups = string.Join(", ", attribute.Value.SS ?? new List<string>());
                    }
                }
            }

            return commandDetails;
        }

        public async Task<string> CheckIfUserIsAuthorised(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string userId,
            string authorisedUsers,
            string authorisedGroups)
        {
            var userIsAuthorised = (authorisedUsers.Contains(userId) == false)
                ? null
                : userId;

            if (userIsAuthorised == null)
            {
                int groupAllowed = 0;
                List<string> usersGroups = new List<string>();

                if (daxUrl.Contains(".com"))
                {
                    var client = GetDaxClient(
                        daxUrl: daxUrl,
                        daxPort: daxPort,
                        region: region);

                    GetItemRequest request = new GetItemRequest();
                    request.TableName = "slack_user";
                    request.Key = new Dictionary<string, AttributeValue>
                    {
                        {"id", new AttributeValue {S = userId}}
                    };

                    List<String> stringList = new List<string>();
                    stringList.Add("groups");

                    request.AttributesToGet = stringList;

                    var item = await client.GetItemAsync(
                        request: request,
                        cancellationToken: new CancellationToken());


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
                }
                else
                {
                    var client = GetDynamoDbClient(
                        accessKey: accessKey,
                        secretKey: secretKey,
                        sessionToken: sessionToken,
                        region: region);

                    GetItemRequest request = new GetItemRequest();
                    request.TableName = "slack_user";
                    request.Key = new Dictionary<string, AttributeValue>
                    {
                        {"id", new AttributeValue {S = userId}}
                    };

                    List<String> stringList = new List<string>();
                    stringList.Add("groups");

                    request.AttributesToGet = stringList;

                    var item = await client.GetItemAsync(
                        request: request,
                        cancellationToken: new CancellationToken());

                    
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
                }
            }

            return userIsAuthorised;
        }

        public async Task<List<string>> GetStringListQueryAsync(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string daxUrl,
            int daxPort,
            string indexName,
            string tableName,
            string key,
            string value,
            string attribute)
        {
            var resultSet = new List<string>();

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
                    {key, new AttributeValue {S = value}}
                };

                var items = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                foreach (var item in items.Item)
                {
                    if (item.Key == attribute)
                    {
                        foreach (var x in item.Value.L)
                        {
                            resultSet.Add(x.S);
                        }
                    }
                }
            }
            else
            {
                var client = GetDynamoDbClient(
                     accessKey: accessKey,
                     secretKey: secretKey,
                     sessionToken: sessionToken,
                     region: region);

                GetItemRequest request = new GetItemRequest();
                request.TableName = tableName;
                request.Key = new Dictionary<string, AttributeValue>
                {
                    {key, new AttributeValue {S = value}}
                };

                var items = await client.GetItemAsync(
                    request: request,
                    cancellationToken: new CancellationToken());

                foreach (var item in items.Item)
                {
                    if (item.Key == attribute)
                    {
                        foreach (var x in item.Value.L)
                        {
                            resultSet.Add(x.S);
                        }
                    }
                }
            }
         
            return resultSet;
        }

        public AmazonDynamoDBClient GetDynamoDbClient(
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