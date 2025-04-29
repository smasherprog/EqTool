using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows;

namespace EQTool.Models
{
    [Serializable]
    public class Trigger
    {
        // regex to support conversion of simplified regex to full "real" regex
        private const string ginaRegexPattern = @"\{(?<xxx>\w+)\}";
        private static Regex ginaRegex = new Regex(ginaRegexPattern, RegexOptions.Compiled);

        // Trigger properties
        public Guid TriggerId { get; set; } = Guid.NewGuid();
        public bool TriggerEnabled { get; set; }
        public string TriggerName { get; set; }

        // the search field
        // contains user-input simplified gina-style regex
        private string _SearchText = string.Empty;
        public string SearchText
        {
            get { return this._SearchText; }
            set
            {
                if (this._SearchText != value)
                {
                    this._SearchText = value;
                    this._TriggerRegex = null;
                }
            }
        }

        // properties to support text to be displayed
        // contains user-input simplified gina-style regex
        public bool DisplayTextEnabled { get; set; }
        public string DisplayText { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ConvertedDisplayText { get { return ProcessOutputText(AudioText); } }

        // properties to support text to be spoken via TTS
        // contains user-input simplified gina-style regex
        public bool AudioTextEnabled { get; set; }
        public string AudioText { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ConvertedAudioText { get { return ProcessOutputText(AudioText); } }

        // the regular expression for this trigger
        private Regex _TriggerRegex;

        [Newtonsoft.Json.JsonIgnore]
        public Regex TriggerRegex
        {
            get
            {
                // delay regex creation until its asked for
                if (this._TriggerRegex == null && !string.IsNullOrWhiteSpace(this._SearchText))
                {
                    // convert search text user input which may contain simplified gina-style regex, i.e.
                    //      {some_name}
                    // to the real regex match input, i.e.
                    //      (?<some_name>[\w` ]+)
                    //
                    // note the regex allows the field to contain
                    //      - \w for any character found in words
                    //      - spaces, to capture multi-word targets, i.e. "Spider Queen D`Zee"
                    //      - the ` back tick, which actually appears in quite a few mob names
                    //
                    var convertedSearchText = this._SearchText;
                    var match = ginaRegex.Match(convertedSearchText);
                    while (match.Success)
                    {
                        string group_name = match.Groups["xxx"].Value;
                        convertedSearchText = ginaRegex.Replace(convertedSearchText, $"(?<{group_name}>[\\w` ]+)", 1);
                        match = match.NextMatch();
                    }

                    // now that we've converted the simplified regex to the real regex pattern, create and return the Regex object
                    this._TriggerRegex = new Regex(convertedSearchText, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }

                return this._TriggerRegex;
            }
        }

        // The user can define search patterns using a simplified regular expression syntax (similar to Gina)
        //       Example:    ^{backstabber} backstabs {target} for {damage} points of damage\.
        // The simplified form gets converted into the real regex expression for use in creating the TriggerRegex
        //       Example:    ^(?<backstabber>[\w` ]+) backstabs (?<target>[\w` ]+) for (?<damage>[\w` ]+) points of damage\.
        // Regular expression "named groups" from the user input are stored in a HashTable, with keys = the group names, and values = the parsed values
        //       Example:    Roger the Rogue backstabs a poor rabbit for 1000 points of damage
        // Resulting hashTable of (key, value) pairs:
        //       (backstabber, Roger the Rogue)
        //       (target, a poor rabbit)
        //       (damage, 1000)
        private readonly Hashtable valueHash = new Hashtable();     // list of named groups and their parsed values that we find in the search text

        // use this function to save the results after a regular expression search has been performed
        // if successful, then save the results into the trigger, for use later
        internal void SaveNamedGroupValues(Match match)
        {
            // walk the Groups list in the regex Match, and save the values we care about
            foreach (Group g in match.Groups)
            {
                // is this named group key already in the hash?  then just reset the value
                if (valueHash.ContainsKey(g.Name))
                {
                    valueHash[g.Name] = g.Value;
                }
                // else add both the named group key and the associated value
                else
                {
                    valueHash.Add(g.Name, g.Value);
                }
            }
        }

        // utility function to merge in the parsed values into the output simplified regex fields
        // example:  Change the output text for a "Sense Heading" trigger from:
        //      "Direction: {direction}"
        // to
        //      "Direction: East"
        private string ProcessOutputText(string inputText)
        {
            string rv = inputText;

            // walk the list of matches, replacing the user match with the real match
            Match match = ginaRegex.Match(rv);
            while (match.Success)
            {
                // Handle match here...
                string group_name = match.Groups["xxx"].Value;

                // this key should be present, but confirm in case user made a typo
                if (valueHash.ContainsKey(group_name))
                {
                    // use regex to replace the gina named group with value from the hashtable
                    // do them one group at a time
                    string replace_text = $"{valueHash[group_name]}";
                    rv = ginaRegex.Replace(rv, replace_text, 1);
                }

                match = match.NextMatch();
            }
            return rv;
        }
    }
}