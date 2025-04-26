using System;
using System.Text.RegularExpressions;

namespace EQTool.Models
{
    [Serializable]
    public class Trigger
    {
        private const string ginaRegexPattern = @"\{(?<xxx>\w+)\}";
        private static Regex ginaRegex = new Regex(ginaRegexPattern, RegexOptions.Compiled);

        public Guid TriggerId { get; set; } = Guid.NewGuid();
        public bool TriggerEnabled { get; set; }
        public string TriggerName { get; set; }
        private string _SearchText = string.Empty;
        public string SearchText
        {
            get { return this._SearchText; }
            set
            {
                if (this._SearchText != value)
                {
                    this._TriggerRegex = null;
                    this._SearchText = value;
                }
            }
        }
        public bool DisplayTextEnabled { get; set; }
        public string DisplayText { get; set; }
        public bool AudioTextEnabled { get; set; }
        public string AudioText { get; set; }

        private Regex _TriggerRegex;
        [Newtonsoft.Json.JsonIgnore]
        public Regex TriggerRegex
        {
            get
            {
                //delay regex creation until its asked for
                if (this._TriggerRegex == null && !string.IsNullOrWhiteSpace(this._SearchText))
                {
                    var pattern = this._SearchText;
                    var match = ginaRegex.Match(pattern);
                    while (match.Success)
                    {
                        string group_name = match.Groups["xxx"].Value;
                        pattern = ginaRegex.Replace(pattern, $"(?<{group_name}>[\\w` ]+)", 1);
                        match = match.NextMatch();
                    }
                    this._TriggerRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }

                return this._TriggerRegex;
            }
        }
    }


}