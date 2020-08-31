using System.Collections.Generic;

namespace LambdaSlackDynamicDataSource.Entities
{
    public class ExternalDataSource
    {
        public class Value
        {
            public string source { get; set; }
            public string lambda_name { get; set; }
            public string table { get; set; }
            public string key { get; set; }
            public string attribute { get; set; }
            public string name { get; set; }
            public string value { get; set; }
        }

        public class RootObject
        {
            public List<Value> values { get; set; }
        }
    }
}
