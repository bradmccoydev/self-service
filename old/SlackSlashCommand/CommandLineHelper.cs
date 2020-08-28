using System;
using System.Threading.Tasks;
using SlackSlashCommand.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlackSlashCommand
{
    public class CommandLineHelper
    {
        public async Task<Arguments> GetCommandLineArguments(
            string args)
        {
            Arguments arguments = new Arguments();

            arguments.Payload = "";

            if (args.Contains(" "))
            {
                var command = GetStringBeforeCharacter(value: args, character: "-");
                arguments.Command = command.Replace(" ","");
                //args.Replace(arguments.Command,"").Replace(" ","");

                // var lines = args.Split(new string[] { "-" }, StringSplitOptions.None);

                // foreach (var line in lines)
                // {
                //     Console.WriteLine(line);
                // }
            }

            Console.WriteLine($"command: {arguments.Command}");

            // if (args.Contains("-payload"))
            // {
            //     var payload = GetStringBetweenTwoCharacters(
            //         value: args,
            //         characterA: "-payload '",
            //         characterB: "'",
            //         characterAIndex: "",
            //         characterbIndex: "");

            //     arguments.Payload = payload;

            //     args = args.Replace($"-payload '{payload}'","");
            // }

            // if (args.Contains("-name"))
            // {
            //     var name = GetStringBetweenTwoCharacters(
            //         value: args,
            //         characterA: "-name '",
            //         characterB: "'",
            //         characterAIndex: "",
            //         characterbIndex: "");

            //         arguments.Name = name;
            // }

            return arguments;
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
