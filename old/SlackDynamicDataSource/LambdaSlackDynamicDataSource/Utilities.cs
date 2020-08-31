using System;
using System.Text;
using System.Security.Cryptography;

namespace LambdaSlackDynamicDataSource
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

        public string GetStateValue(
            string state,
            string name)
        {
            var stateLines = state.Split(new string[] { "'," }, StringSplitOptions.None);

            string value = "";

            foreach (var stateColumn in stateLines)
            {
                var stateAttribute = GetStringBeforeCharacter(stateColumn, "':").Replace("'", "").Replace("\"state\":", "");
                var stateValues = GetStringAfterCharacter(stateColumn, "':").Replace("'", "");

                if (stateAttribute == name)
                {
                    value = stateValues.Replace("+"," ");
                }
            }

            return value;
        }

        public string UrlDecodeString(string value)
        {
            return Uri.UnescapeDataString(value);
        }

        public string UrlEncodeString(string value)
        {
            return Uri.EscapeUriString(value);
        }

        public string GetStringBetweenTwoCharacters(
            string value,
            string characterA,
            string characterB,
            string characterAIndex,
            string characterbIndex)
        { 
            int posA = value.IndexOf(characterA);
            int posB = value.LastIndexOf(characterB);

            if (characterbIndex == "first")
            {
                posB = value.IndexOf(characterB);
            }

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + characterA.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
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
    }
}