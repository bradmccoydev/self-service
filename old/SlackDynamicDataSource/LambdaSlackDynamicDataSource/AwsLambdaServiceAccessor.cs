﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

namespace LambdaSlackDynamicDataSource
{
    public class AwsLambdaServiceAccessor
    {
        public async Task<List<string>> InvokeLambdaFunction(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region,
            string functionName,
            string payload,
            string requestResponse)
        {
            var list = new List<string>();

            var client = await GetLambdaClient(
                accessKey: accessKey,
                secretKey: secretKey,
                sessionToken: sessionToken,
                region: region);

            var invocationType = (requestResponse == "true")
                ? InvocationType.RequestResponse
                : InvocationType.Event;

            InvokeRequest ir = new InvokeRequest
            {
                FunctionName = functionName,
                InvocationType = invocationType,
                Payload = payload
            };

            InvokeResponse response = await client.InvokeAsync(ir);
            // Console.WriteLine($"LogResult:{response.LogResult}");
            // Console.WriteLine($"Payload:{response.Payload}");

            string responseString = response.HttpStatusCode.ToString();

            // if (requestResponse == "true")
            // {
            //     var sr = new StreamReader(response.Payload);
            //     JsonReader reader = new JsonTextReader(sr);

            //     var serilizer = new JsonSerializer();
            //     var test = serilizer.Deserialize(reader).ToString();
            //     responseString = serilizer.Deserialize(reader).ToString();
            // }

            Console.WriteLine($"Lambda: " + responseString);
            return list;

        }

        public async Task<AmazonLambdaClient> GetLambdaClient(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region)
        {
            AmazonLambdaClient lambdaClient = new AmazonLambdaClient();

            var iamRegion = RegionEndpoint.GetBySystemName(region);

            if (sessionToken != "")
            {
                SessionAWSCredentials awsCreds = new SessionAWSCredentials(
                    awsAccessKeyId: accessKey,
                    awsSecretAccessKey: secretKey,
                    token: sessionToken);

                lambdaClient = new AmazonLambdaClient(
                    awsCreds,
                    iamRegion);
            }
            if (sessionToken == ""
                && accessKey != "")
            {
                BasicAWSCredentials awsCreds = new BasicAWSCredentials(
                    accessKey: accessKey,
                    secretKey: secretKey);

                lambdaClient = new AmazonLambdaClient(
                    awsCreds, iamRegion);
            }
            if (accessKey == "")
            {
                lambdaClient = new AmazonLambdaClient(iamRegion);
            }

            return lambdaClient;
        }
    }
}