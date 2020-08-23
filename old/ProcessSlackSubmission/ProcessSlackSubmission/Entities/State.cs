using System.Collections.Generic;

namespace ProcessSlackSubmission.Entities
{
    public class State
    {
        public string Payload { get; set; }
        
        public int Dialog { get; set; }

        public int NumberOfDialogs { get; set; }

        public string number_of_dialogs { get; set; }

        public bool IsLastDialog { get; set; }

        public Dictionary<string,string> Dictionary { get; set; } 
        
    }
}
