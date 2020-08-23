﻿using System.Collections.Generic;

namespace ProcessSlackSubmission.Entities.Block
{
    public class FieldBlock
    {
        public class Field
        {
            public string type { get; set; }
            public string text { get; set; }
        }

        public class RootObject
        {
            public string type { get; set; }
            public List<Field> fields { get; set; }
        }
    }
}
