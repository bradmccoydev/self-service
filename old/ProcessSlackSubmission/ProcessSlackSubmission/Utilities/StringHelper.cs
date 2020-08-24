using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProcessSlackSubmission.Entities;

namespace ProcessSlackSubmission.Utilities
{
    public class StringHelper
    {
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
            string characterB)
        {
            var bra = value.Split(new string[] { "\"submission\":" }, StringSplitOptions.None)[1]
            .Split('}')[0]
            .Trim();

            return bra.Replace("{", "").Trim();
        }
        public State AddStateToJsonPayload(
            string state,
            string json)
        {
            json = json.Replace("\n", "")
            .Replace("\" , \"", "\",\"")
            .Replace("\", \"", "\",\"")
            .Replace("\" ", "\"")
            .Replace(" \"", "\"");

            string attribute = "";
            string values = "";

            var lines = json.Split(new string[] { "\"," }, StringSplitOptions.None);
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var column in lines)
            {
                attribute = GetStringBeforeCharacter(column, "\":").Replace("\"", "").Trim();
                values = GetStringAfterCharacter(column, ":\"").Replace("\"", "").Trim();

                if (dict.ContainsKey(attribute))
                {
                    dict.Remove(attribute);
                    dict.Add(key: attribute, value: values);
                }

                if (!dict.ContainsKey(attribute)
                    && values != "null")
                {
                    dict.Add(key: attribute, value: values);
                }
            }

            State newState = new State();

            if (state != ""
                && state != null)
            {
                var stateLines = state.Split(new string[] { "'," }, StringSplitOptions.None);

                foreach (var stateColumn in stateLines)
                {
                    var stateAttribute = GetStringBeforeCharacter(stateColumn, "':").Replace("'", "").Replace("\"state\":", "");
                    var stateValues = GetStringAfterCharacter(stateColumn, "':").Replace("'", "");

                    if (dict.ContainsKey(stateAttribute))
                    {
                        dict.Remove(stateAttribute);
                        dict.Add(key: stateAttribute, value: stateValues);
                    }

                    if (!dict.ContainsKey(stateAttribute))
                    {
                        dict.Add(key: stateAttribute, value: stateValues);
                    }

                    if (stateAttribute == "dialog")
                    {
                        var dialog = Int32.Parse(stateValues);
                        newState.Dialog = dialog;
                        dialog = dialog + 1;
                        dict.Remove(stateAttribute);
                        dict.Add(key: stateAttribute, value: dialog.ToString());
                    }

                    if (stateAttribute == "number_of_dialogs")
                    {
                        newState.NumberOfDialogs = Int32.Parse(stateValues);
                    }
                }
            }

            if ((newState.Dialog + 1) > newState.NumberOfDialogs)
            {
                newState.IsLastDialog = true;

                dict.Remove("id");
                dict.Remove("dialog");
                dict.Remove("number_of_dialogs");
                dict.Remove("callback_id");
                //dict.Remove("self_service_environment");
            }

            newState.Payload = JsonConvert.SerializeObject(dict).Replace(",\"\":\"\"", "");
            newState.Dictionary = dict;

            return newState;
        }

        public string CleanState(
            string state)
        {
            var newState = state.Replace("':'", "\":\"").Replace("','", "\",\"");
            newState = newState.TrimStart('\'');
            newState = newState.TrimEnd('\'');
            newState = "\"" + newState + "\"";
            return newState;
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
