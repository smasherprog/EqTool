using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services
{
    // class for simple non-timer Triggers
    public class UserDefinedTrigger
    {
        public int      TriggerID { get; set; }
        public bool     TriggerEnabled { get; set; }
        public string   TriggerName { get; set; }
        public string   SearchTest { get; set; }
        public bool     TextEnabled { get; set; }
        public string   DisplayText { get; set; }
        public bool     AudioEnabled { get; set; }
        public string   AudioText { get; set; }
    }
}
