using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProcessSlackSubmission.Entities;
using ProcessSlackSubmission.Entities.Block;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProcessSlackSubmission
{
    public class SlackServiceAccessor
    {
        public async Task<String> GetUserEmail(
            string token,
            string user)
        {
            string url = "https://slack.com/api/users.profile.get?token=" + token + $"&user={user}&pretty=1";
            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");
            var stringTask = client.GetStringAsync(url);
            var json = await stringTask;
            Console.WriteLine(json);
            var userDetails = JsonConvert.DeserializeObject<SlackUser.RootObject>(json);
            return userDetails.profile.email;
        }

        public async Task<string> OpenDialog(
            string token,
            string dialogJson,
            string triggerId)
        {
            dialogJson = JObject.Parse(dialogJson).ToString(Newtonsoft.Json.Formatting.None);
            var urlEncode = System.Web.HttpUtility.UrlEncode(dialogJson);

            string url = "https://slack.com/api/dialog.open?token=" + token + $"&dialog={urlEncode}&trigger_id={triggerId}&pretty=1";

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(url, content);
            var contents = await response.Content.ReadAsStringAsync();

            return contents;
        }

        public string BuildSlackMessageBlock(
            string title,
            string callbackId,
            string greenActionName,
            string redActionName,
            Dictionary<string,string> payload)
        {
            var textBlock = new TextBlock.RootObject();
            textBlock.type = "section";

            var text = new TextBlock.Text();
            text.type = "mrkdwn";
            text.text = title;

            textBlock.text = text;

            var fieldBlock = new FieldBlock.RootObject();
            fieldBlock.type = "section";

            var fieldList = new List<FieldBlock.Field>();
            
            if (payload.Count > 0)
            {
                foreach (var state in payload)
                {
                    if (state.Key != "number_of_dialogs"
                        && state.Key != "dialog"
                        && state.Key != "id"
                        && state.Key != "self_service_environment")
                    {
                        var field = new FieldBlock.Field();
                        field.type = "mrkdwn";
                        field.text = $"*{state.Key}*\n{state.Value}";

                        fieldList.Add(field);
                    }
                }
            }

            fieldBlock.fields = fieldList;

            var actionBlock = new ActionBlock.RootObject();
            actionBlock.type = "actions";

            var elements = new List<ActionBlock.Element>();

            string statePayload = "";

            foreach (var item in payload)
            {
                statePayload = statePayload + $"'{item.Key}':'{item.Value}',";
            }

            statePayload = statePayload + $"'callback_id':'{callbackId}'";

            var actionBlockElementConfirm = new ActionBlock.Element();
            actionBlockElementConfirm.type = "button";
            actionBlockElementConfirm.style = "primary";
            actionBlockElementConfirm.value = statePayload;

            var actionTextConfirm = new ActionBlock.Text();
            actionTextConfirm.type = "plain_text";
            actionTextConfirm.emoji = true;
            actionTextConfirm.text = greenActionName;

            actionBlockElementConfirm.text = actionTextConfirm;

            elements.Add(actionBlockElementConfirm);

            var actionBlockElementCancel = new ActionBlock.Element();
            actionBlockElementCancel.type = "button";
            actionBlockElementCancel.style = "danger";
            actionBlockElementCancel.value = statePayload;

            if (redActionName != "")
            {
                var actionTextCancel = new ActionBlock.Text();
                actionTextCancel.type = "plain_text";
                actionTextCancel.emoji = true;
                actionTextCancel.text = redActionName;

                actionBlockElementCancel.text = actionTextCancel;

                elements.Add(actionBlockElementCancel);
            }

            actionBlock.elements = elements;

            var textBlockJson = JsonConvert.SerializeObject(textBlock, Formatting.None);
            var fieldBlockJson = JsonConvert.SerializeObject(fieldBlock, Formatting.None);
            var actionBlockJson = JsonConvert.SerializeObject(actionBlock, Formatting.None);

            return $"[{textBlockJson},{fieldBlockJson},{actionBlockJson}]";
        }

        public async Task<string> SendMessageBlockAsync(
            string token,
            string channel,
            string channelType,
            string user,
            string block)
        {
            var urlEncode = System.Web.HttpUtility.UrlEncode(block);

            var url = "https://slack.com/api/chat.postEphemeral?token=" + token + $"&channel=%23{channel}&user={user}&blocks={urlEncode}&pretty=1";

            if (user == "")
            {
                url = "https://slack.com/api/chat.postMessage?token=" + token + $"&channel=%23{channel}&blocks={urlEncode}&text=hello&pretty=1";
            }

            if (channelType == "directmessage")
            {
                url = "https://slack.com/api/chat.postMessage?token=" + token + $"&channel={channel}&blocks={urlEncode}&text={block}&pretty=1";
            }

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            string payLoad = "";

            var contenta = new StringContent(payLoad, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, contenta);
            var contents = await response.Content.ReadAsStringAsync();

            var messageDetails = JsonConvert.DeserializeObject<SlackMessageResponse.RootObject>(contents);

            return messageDetails.message_ts;
        }

        public async Task<string> PostSlackMessageAsync(
            string token,
            string channel,
            string text)
        {
            var url = "https://slack.com/api/chat.postMessage?token=" + token + $"&channel=%23{channel}&text={text}&pretty=1";

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            string payLoad = "";

            var contenta = new StringContent(payLoad.ToString(), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, contenta);
            var contents = await response.Content.ReadAsStringAsync();
            var messageDetails = JsonConvert.DeserializeObject<SlackMessageResponse.RootObject>(contents);

            return messageDetails.message_ts;
        }

        public async Task<string> SendSlackMessageAsync(
            string url,
            string token,
            string icon,
            string channel,
            string message)
        {
            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            JObject payLoad = new JObject(
                new JProperty("channel", channel),
                new JProperty("text", message),
                new JProperty("username", "Brad McCoy"));

            var content = new StringContent(payLoad.ToString(), Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            var contents = await response.Content.ReadAsStringAsync();
            return contents;
        }

        public async Task<string> PostSlackMessageThreadAsync(
            string token,
            string channel,
            string threadTs,
            string text)
        {
            var url = "https://slack.com/api/chat.postMessage?token=" + token + $"&channel=%23{channel}&thread_ts={threadTs}&text={text}&pretty=1";

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            string payLoad = "";

            var contenta = new StringContent(payLoad.ToString(), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, contenta);
            var contents = await response.Content.ReadAsStringAsync();
            var messageDetails = JsonConvert.DeserializeObject<SlackMessageResponse.RootObject>(contents);

            return messageDetails.message_ts;
        }

        public async Task<string> SendPostEphemeralAsync(
            string token,
            string channel,
            string user,
            string text,
            string attachments)
        {
            var urlEncode = System.Web.HttpUtility.UrlEncode(attachments);

            string url = "https://slack.com/api/chat.postEphemeral?token=" + token + $"&channel=%23{channel}&text={text}&user={user}&attachments={urlEncode}&pretty=1";

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            string payLoad = "";

            var content = new StringContent(payLoad.ToString(), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var contents = await response.Content.ReadAsStringAsync();
            return contents;
        }

        public async Task<string> PostSlackDirectMessage(
            string token,
            string user,
            string text)
        {
            string getUrl = "https://slack.com/api/im.open?token=" + token + $"&user={user}&pretty=1";

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            var stringTask = client.GetStringAsync(getUrl);
            var json = await stringTask;

            var directMessageChannel = JsonConvert.DeserializeObject<SlackDirectMessageChannel.RootObject>(json);

            string posturl = "https://slack.com/api/chat.postMessage?token=" + token + $"&channel={directMessageChannel.channel.id}&text={text}&pretty=1";

            var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(posturl, content);

            return directMessageChannel.channel.id;
        }

        public async Task<string> GetSlackDirectMessageChannelNameById(
            string token,
            string user)
        {
            string getUrl = "https://slack.com/api/im.open?token=" + token + $"&user={user}&pretty=1";

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            var stringTask = client.GetStringAsync(getUrl);
            var json = await stringTask;

            var directMessageChannel = JsonConvert.DeserializeObject<SlackDirectMessageChannel.RootObject>(json);

            return directMessageChannel.channel.id;
        }

        public async Task<string> DeleteSlackMessageAsync(
            string token,
            string channel,
            string ts)
        {
            string posturl = "https://slack.com/api/chat.delete?token=" + token + $"&ts={ts}&channel={channel}&as_user=true&pretty=1";

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PostAsync(posturl, content);

            var contents = await response.Content.ReadAsStringAsync();
            //var messageDetails = JsonConvert.DeserializeObject<SlackMessageResponse.RootObject>(contents);


            return contents;
        }

        public async Task<string> GetChannelNameById(
           string token,
           string id,
           string user,
           string channelType)
        {
            string channelName = "temenos-self-service";

            if (channelType == "directmessage")
            {
                string getUrl = "https://slack.com/api/im.open?token=" + token + $"&user={user}&pretty=1";

                var client = GetHttpClientWithAuthHeader(
                    token: token,
                    mediaType: "application/json");

                var stringTask = client.GetStringAsync(getUrl);
                channelName = id;
            }

            if (channelType == "privategroup")
            {
                channelName = await GetSlackGroupNameById(
                        token: token,
                        groupId: id);
            }

            if (channelType != "privategroup"
                && channelType != "directmessage")
            {
                channelName = await GetSlackChannelNameById(
                        token: token,
                        channelId: id);
            }

            return channelName;
        }

        public async Task<string> GetSlackChannelNameById(
            string token,
            string channelId)
        {
            string url = "https://slack.com/api/channels.list";
            string name = "Channel Not Found";
            //string cursor = 1;

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            //do
            //{
                //url = (cursor == "1") ? url : $"{url}?cursor={cursor}";
                var stringTask = client.GetStringAsync(url);
                var json = await stringTask;
                var channels = JsonConvert.DeserializeObject<SlackChannel.RootObject>(json);
  
                foreach (var channel in channels.channels)
                {
                    if (channel.id == channelId)
                    {
                        name = channel.name;
                    }
                }
            //} while (cursor != "");
           

            return name;
        }

        public async Task<string> GetSlackGroupNameById(
            string token,
            string groupId)
        {
            string url = "https://slack.com/api/groups.list";
            string name = "Group Not Found";
            string cursor = "1";

            var client = GetHttpClientWithAuthHeader(
                token: token,
                mediaType: "application/json");

            do
            {
                try
                {
                    url = (cursor == "1") ? url : $"https://slack.com/api/groups.list?" + $"cursor={cursor}";
                    var stringTask = client.GetStringAsync(url);
                    var json = await stringTask;
                    var groups = JsonConvert.DeserializeObject<SlackGroup.RootObject>(json);
                    cursor = groups.response_metadata.next_cursor;

                    foreach (var group in groups.groups)
                    {
                        if (group.id == groupId)
                        {
                            name = group.name;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    continue;
                }

            } while (cursor != "");

            return name;
        }

        public HttpClient GetHttpClientWithAuthHeader(
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
    }
}
