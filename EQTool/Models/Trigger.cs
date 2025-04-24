using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EQTool.Models
{
    [Serializable]
    public class Trigger : INotifyPropertyChanged
    {
        private const string ginaRegexPattern = @"\{(?<xxx>\w+)\}";
        private static Regex ginaRegex = new Regex(ginaRegexPattern, RegexOptions.Compiled);

        private bool _TriggerEnabled;
        public bool TriggerEnabled
        {
            get { return this._TriggerEnabled; }
            set
            {
                if (this._TriggerEnabled != value)
                {
                    this._TriggerEnabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _TriggerName = string.Empty;
        public string TriggerName
        {
            get { return this._TriggerName; }
            set
            {
                if (this._TriggerName != value)
                {
                    this._TriggerName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SearchText = string.Empty;
        public string SearchText
        {
            get { return this._SearchText; }
            set
            {
                if (this._SearchText != value)
                {
                    if (this._TriggerRegex != null)
                    {
                        this._TriggerRegex = BuildRegex(value);
                        this._SearchText = value;
                    }
                    else
                    {
                        this._SearchText = value;
                    }
                    this.OnPropertyChanged();
                }
            }
        }

        private static Regex BuildRegex(string pattern)
        {
            var match = ginaRegex.Match(pattern);
            while (match.Success)
            {
                string group_name = match.Groups["xxx"].Value;
                pattern = ginaRegex.Replace(pattern, $"(?<{group_name}>[\\w` ]+)", 1);
                match = match.NextMatch();
            }
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private Regex _TriggerRegex;
        [Newtonsoft.Json.JsonIgnore]
        public Regex TriggerRegex
        {
            get
            {
                //delay regex creation until its asked for
                if (this._TriggerRegex == null && !string.IsNullOrWhiteSpace(this._SearchText))
                {
                    try
                    {
                        this._TriggerRegex = BuildRegex(this._SearchText);
                    }
                    catch
                    {
                        this._TriggerRegex = null;
                    }
                }

                return this._TriggerRegex;
            }
        }

        public bool DisplayTextEnabled { get; set; }
        public string DisplayText { get; set; }

        public bool AudioEnabled { get; set; }
        public string AudioText { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public static class TriggerExtentions
    {
        private const string ginaRegexPattern = @"\{(?<xxx>\w+)\}";
        private static Regex ginaRegex = new Regex(ginaRegexPattern, RegexOptions.Compiled);
        public static bool Match(this Trigger trigger, string line)
        {
            if (trigger.TriggerRegex == null || !trigger.TriggerEnabled)
            {
                return false;
            }

            var namevaluePairs = new List<(string Name, string Value)>();
            var match = trigger.TriggerRegex.Match(line);
            if (match.Success)
            {
                foreach (Group g in match.Groups)
                {
                    if (!namevaluePairs.Any(a => string.Equals(a.Name, g.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        namevaluePairs.Add((g.Name, g.Value));
                    }
                }
            }

            if (trigger.DisplayTextEnabled)
            {
                trigger.DisplayText = ProcessOutputText(trigger.DisplayText, namevaluePairs);
            }

            if (trigger.AudioEnabled)
            {
                trigger.AudioText = ProcessOutputText(trigger.AudioText, namevaluePairs);
            }

            return match.Success;
        }

        // utility function to merge in the parsed values into the output simplified regex fields
        // example:  Change the output text for a "Sense Heading" trigger from:
        //      "Direction: {direction}"
        // to
        //      "Direction: East"
        private static string ProcessOutputText(string inputText, List<(string Name, string Value)> namevaluePairs)
        {
            string rv = inputText;

            // walk the list of matches, replacing the user match with the real match
            Match match = ginaRegex.Match(rv);
            while (match.Success)
            {
                // Handle match here...
                string group_name = match.Groups["xxx"].Value;

                // this key should be present, but confirm in case user made a typo
                var nameValue = namevaluePairs.FirstOrDefault(a => string.Equals(a.Name, group_name, StringComparison.OrdinalIgnoreCase));
                if (nameValue != default)
                {
                    // use regex to replace the gina named group with value from the hashtable
                    // do them one group at a time
                    string replace_text = $"{nameValue.Value}";
                    rv = ginaRegex.Replace(rv, replace_text, 1);
                }

                match = match.NextMatch();
            }
            return rv;
        }
    }
}