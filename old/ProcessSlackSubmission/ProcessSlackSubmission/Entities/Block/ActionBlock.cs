using System.Collections.Generic;

namespace ProcessSlackSubmission.Entities.Block
{
    public class ActionBlock
    {
        public class Text
        {
            public string type { get; set; }
            public bool emoji { get; set; }
            public string text { get; set; }
        }

        public class Element
        {
            public string type { get; set; }
            public Text text { get; set; }
            public string style { get; set; }
            public string value { get; set; }
        }

        public class RootObject
        {
            public string type { get; set; }
            public List<Element> elements { get; set; }
        }
    }
}
