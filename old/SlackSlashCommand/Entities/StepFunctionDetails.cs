using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlackSlashCommand.Entities
{
    public sealed class StepFunctionDetails
    {
        public string StateMachineConfig { get; set; }
        public string StateMachineArn { get; set; }
    }
}