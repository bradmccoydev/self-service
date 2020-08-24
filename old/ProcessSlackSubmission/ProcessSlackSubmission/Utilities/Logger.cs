using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading;
using ProcessSlackSubmission.Entities;
using Newtonsoft.Json;

namespace ProcessSlackSubmission.Utilities
{
    public class Logger
    {
        public async Task Log(
            string region,
            string id,
            string trackingId,
            string command,
            string user,
            string team,
            string channel,
            string payload,
            string endpoint,
            List<string> approvers,
            string status)
            {
                var date = DateTime.UtcNow;
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var dateTimeUnix = Convert.ToInt64((date - epoch).TotalSeconds);

                string format = "yyyy-MM-dd hh:mm:ss tt";
                string dateTimeLoadedUtc = date.ToString(format);

                var dt = new DataTable();

                dt.Columns.Add("id", typeof(String));
                dt.Columns.Add("command", typeof(String));
                dt.Columns.Add("user", typeof(String));
                dt.Columns.Add("team", typeof(String));
                dt.Columns.Add("channel", typeof(String));
                dt.Columns.Add("payload", typeof(String));
                dt.Columns.Add("status", typeof(String));
                dt.Columns.Add("date_time_unix", typeof(String));
                dt.Columns.Add("date_time_loaded_utc", typeof(String));
                dt.Columns.Add("tracking_id", typeof(String));

                DataRow dr = dt.NewRow();

                dr["id"] = id;
                dr["command"] = command;
                dr["user"] = user;
                dr["team"] = team;
                dr["channel"] = channel;
                dr["payload"] = payload.Replace(",", "|").Replace("\n", "").Replace("\r", "");
                dr["status"] = status;
                dr["date_time_unix"] = dateTimeUnix;
                dr["date_time_loaded_utc"] = dateTimeLoadedUtc;
                dr["tracking_id"] = trackingId;

                dt.Rows.Add(dr);

                var csvByteArray = ConvertDataTableToCsv(dt: dt);

                var folderName = $"/year={date.Year}/month={date.ToString("MM")}/day={date.ToString("dd")}";

                await UploadFileToS3BucketFromByteArrayAsync(
                    bucketName: "bradmccoy.io",
                    folderName: folderName,
                    region: region,
                    targetFileName: $"{id.Replace(".","")}.csv",
                    targetFileBytes: csvByteArray,
                    contentType: "txt/csv");

            if (status == "Completed")
            {
                Submission submission = new Submission();
                submission.id = id;
                submission.user = user;
                submission.team = team;
                submission.channel = channel;
                submission.command = command;
                submission.payload = payload;
                submission.endpoint = endpoint;
                submission.approvers = approvers;
                submission.status = status;
                submission.date_time_unix = dateTimeUnix;
                submission.date_time_utc = DateTime.UtcNow.ToString(format);
                submission.tracking_id = trackingId;

                var jsonString = JsonConvert.SerializeObject(submission);

                await PutItemJsonAsync(
                    region: region,
                    tableName: "submission",
                    json: jsonString);

                LogEntry logEntry = new LogEntry();
                logEntry.id = id;
                logEntry.tracking_id = trackingId;
                logEntry.datetime = dateTimeUnix.ToString();
                logEntry.endpoint = endpoint;
                logEntry.message = payload;
                logEntry.step = "0";
                logEntry.type = "Log";
                logEntry.endpoint = endpoint;

                var logEntryJson = JsonConvert.SerializeObject(logEntry);

                await PutItemJsonAsync(
                    region: region,
                    tableName: "logs-production",
                    json: logEntryJson);
            }
        }

        public byte[] ConvertDataTableToCsv(DataTable dt)
        {
            var maxColumnNumber = dt.Columns.Count - 1;
            var currentRow = new List<string>(maxColumnNumber);
            var totalRowCount = dt.Rows.Count - 1;
            var currentRowNum = 0;

            var memory = new MemoryStream();

            using (var writer = new StreamWriter(memory, Encoding.ASCII))
            {
                while (currentRowNum <= totalRowCount)
                {
                    BuildRowFromDataTable(dt, currentRow, currentRowNum, maxColumnNumber);
                    WriteRecordToFile(currentRow, writer, currentRowNum, totalRowCount);
                    currentRow.Clear();
                    currentRowNum++;
                }
            }

            return memory.ToArray();
        }

        private void BuildRowFromDataTable(
            DataTable dt,
            List<string> currentRow,
            int currentRowNum,
            int maxColumnNumber)
        {
            for (int i = 0; i <= maxColumnNumber; i++)
            {
                var cell = dt.Rows[currentRowNum][i].ToString();
                if (cell == null)
                {
                    AddCellValue(string.Empty, currentRow);
                }
                else
                {
                    if (cell == null)
                    {
                        cell = string.Empty;
                    }

                    AddCellValue(cell, currentRow);
                }
            }
        }

        private void AddCellValue(string s, List<string> record)
        {
            if (s.Contains("000+0000") && s.Contains("T"))
            {
                s = s.Replace("T", " ").Replace(".000+0000", " ");
            }

            record.Add(string.Format("{0}{1}{0}", '"', s));
        }

        private void WriteRecordToFile(
            List<string> record,
            StreamWriter sw,
            int rowNumber,
            int totalRowCount)
        {
            var commaDelimitedRecord = ConvertListToDelimitedString(
                    list: record,
                    delimiter: ",",
                    insertSpaces: false,
                    qualifier: "",
                    duplicateTicksForSQL: false);

            if (rowNumber == totalRowCount)
            {
                sw.Write(commaDelimitedRecord);
            }
            else
            {
                sw.WriteLine(commaDelimitedRecord);
            }
        }

        public string ConvertListToDelimitedString(
            List<string> list,
            string delimiter = ":",
            bool insertSpaces = false,
            string qualifier = "",
            bool duplicateTicksForSQL = false)
        {
            var result = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                string initialStr = duplicateTicksForSQL ? DuplicateTicksForSql(list[i]) : list[i];
                result.Append((qualifier == string.Empty)
                    ? initialStr
                    : string.Format("{1}{0}{1}", initialStr, qualifier));
                if (i < list.Count - 1)
                {
                    result.Append(delimiter);
                    if (insertSpaces)
                    {
                        result.Append(' ');
                    }
                }
            }
            return result.ToString();
        }

        private string DuplicateTicksForSql(string s)
        {
            return s.Replace("'", "''");
        }

        public async Task UploadFileToS3BucketFromByteArrayAsync(
            string bucketName,
            string folderName,
            string region,
            string targetFileName,
            byte[] targetFileBytes,
            string contentType)
        {
            var iamRegion = RegionEndpoint.GetBySystemName(region);
            var s3Client = new AmazonS3Client(iamRegion);

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

                var fileTransferUtility = new TransferUtility(s3Client);
                fileTransferUtility.Upload(uploadRequest);

            }
        }

        public async Task PutItemJsonAsync(
            string region,
            string tableName,
            string json)
        {
            var iamRegion = RegionEndpoint.GetBySystemName(region);
            var dynamoDBClient = new AmazonDynamoDBClient(iamRegion);

            var item = Document.FromJson(json);

            var table = Table.LoadTable(dynamoDBClient, tableName);

            var response = await table.PutItemAsync(
                doc: item,
                cancellationToken: new CancellationToken());
        }


        public async Task<AmazonDynamoDBClient> GetDynamoDbClient(
            string accessKey,
            string secretKey,
            string sessionToken,
            string region)
        {
            AmazonDynamoDBClient dynamoDBClient = new AmazonDynamoDBClient();

            var iamRegion = RegionEndpoint.GetBySystemName(region);
            dynamoDBClient = new AmazonDynamoDBClient(iamRegion);


            return dynamoDBClient;

        }

    }
}
