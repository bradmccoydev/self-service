using System;
namespace SlackSlashCommand.Entities.Block
{
    public class TextBlock
    {
        public class Text
        {
            public string type { get; set; }
            public string text { get; set; }
        }

        public class RootObject
        {
            public string type { get; set; }
            public Text text { get; set; }
        }
    }
}
