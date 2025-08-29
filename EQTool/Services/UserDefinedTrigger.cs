using System.Text.RegularExpressions;

namespace EQTool.Services
{
    public class UserDefinedTrigger
    {
        public int TriggerID { get; set; }
        public bool TriggerEnabled { get; set; }
        public string TriggerName { get; set; }
        public Regex TriggerRegex { get; set; }

        public string SearchText { get; set; }

        public bool TextEnabled { get; set; }

        public string DisplayText { get; set; }

        public bool AudioEnabled { get; set; }

        public string AudioText { get; set; }
    }
}
