using System;
using System.Text;
using System.Security.Cryptography;

namespace SlackSlashCommand
{
    public class Utilities
    {
        public string HmacSha256Digest(
            string message,
            string secret)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            HMACSHA256 cryptographer = new HMACSHA256(keyBytes);

            byte[] bytes = cryptographer.ComputeHash(messageBytes);

            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public bool ValidateTimeStamp(long slackTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            bool withinTimeRange = false;
            var difference = unixTimestamp - slackTime;

            if (difference > 0
                && difference < 20)
            {
                withinTimeRange = true;
            }

            return withinTimeRange;
        }

        public string JsonToUrlString(
            string json)
        {
            json = json.Replace("{", "").Replace("}", "");

            string attribute = "";
            string values = "";

            var lines = json.Split(new string[] { "\"," }, StringSplitOptions.None);
            string urlEncoded = "";

            foreach (var column in lines)
            {
                attribute = GetStringBeforeCharacter(column, "\":").Replace("\"", "").Trim();
                values = GetStringAfterCharacter(column, "\":").Replace("\"", "").Trim();

                urlEncoded = urlEncoded + "&" + attribute + "=" + Uri.EscapeDataString(values);
            }

            return urlEncoded.TrimStart('&');
        }

        public string GetStringBeforeCharacter(
            string value,
            string character)
        {
            int posA = value.IndexOf(character);

            if (posA == -1)
            {
                return "";
            }

            return value.Substring(0, posA);
        }

        public string GetStringAfterCharacter(
            string value,
            string character)
        {
            int posA = value.LastIndexOf(character);

            if (posA == -1)
            {
                return "";
            }

            int adjustedPosA = posA + character.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }

            return value.Substring(adjustedPosA);
        }
    }
}