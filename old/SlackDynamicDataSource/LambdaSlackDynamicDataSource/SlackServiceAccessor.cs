using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LambdaSlackDynamicDataSource
{
    public class SlackServiceAccessor
    {
        public string ConvertStringListToSlackObject(
            List<string> list)
        {
            RootObject rootObject = new RootObject();
            var options = new List<Option>();

            foreach (var line in list)
            {
                var option = new Option();
                string label = "";

                if (line.Length > 70)
                {
                    label = line.Remove(70);
                    Console.WriteLine($"label: {label}, line: {line}");
                }

                option.label = (label == "") ? line : label;
                option.value = line;

                options.Add(option);
            }

            rootObject.options = options;

            return JsonConvert.SerializeObject(rootObject).Replace("\"", "'");
        }

        public async Task SendSlackMessageAsync(
            string url,
            string token,
            string channel,
            string message)
        {
            var client = GetHttpClientWithAuthHeaderAsync(
                token: token,
                mediaType: "application/json");

            JObject payLoad = new JObject(
                new JProperty("channel", channel),
                new JProperty("text", message),
                new JProperty("username", "Brad McCoy"));

            var contenta = new StringContent(payLoad.ToString(), Encoding.UTF8, "application/json");
            var result = await client.PutAsync(url, contenta);
        }

        public HttpClient GetHttpClientWithAuthHeaderAsync(
            string token,
            string mediaType)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(mediaType));
            TimeSpan ts = TimeSpan.FromSeconds(1000);
            httpClient.Timeout = ts;

            return httpClient;
        }

        public class Option
        {
            public string label { get; set; }
            public string value { get; set; }
        }

        public class RootObject
        {
            public List<Option> options { get; set; }
        }
    }
}
