using System;
namespace SlackSlashCommand.Entities.Block
{
    public class BlockState
    {
        public string id { get; set; }

        public string callback_id { get; set; }

        public int Dialog { get; set; }

        public int number_of_dialogs { get; set; }
    }
}