using System;
namespace SlackSlashCommand.Entities
{
    public class Arguments
    {
        public string Command { get; set; }
        public string Name { get; set; }
        public string Payload { get; set; }
    }
}
