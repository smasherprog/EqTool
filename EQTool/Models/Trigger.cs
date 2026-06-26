using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace EQTool.Models
{
    [Serializable]
    public class Trigger
    {
        // regex to support conversion of simplified regex to full "real" regex
        private const string placeholderRegexPattern = @"\{(?<xxx>\w+)\}";
        private static readonly Regex placeholderRegex = new Regex(placeholderRegexPattern, RegexOptions.Compiled);

        private string _PlayerName { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonIgnore]
        public string PlayerName
        {
            get => _PlayerName;
            set
            {
                if (_PlayerName != value)
                {
                    _PlayerName = value;
                    if (_hasContextToken)
                    {
                        // regex needs to be recompiled if it contains the {c} macro, since that macro is replaced with the current PlayerName
                        _TriggerRegex = null;
                    }
                }
            }
        }

        // Trigger properties
        public Guid TriggerId { get; set; } = Guid.NewGuid();
        public bool TriggerEnabled { get; set; }
        public string TriggerName { get; set; }

        public Guid? FolderId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public bool IsBuiltIn { get; set; }

        public string BuiltInId { get; set; }

        // Organizational/category label.
        public string Category { get; set; } = "Default";
        // Free-form user notes.
        public string Comments { get; set; } = string.Empty;

        public bool? UseRegex { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public bool EffectiveUseRegex => UseRegex ?? true;

        public TriggerOutput Basic { get; set; }

        // Timer / counter configuration. Null == not configured.
        public TriggerTimer Timer { get; set; }
        public TriggerTimerEnding TimerEnding { get; set; }
        public TriggerTimerEnded TimerEnded { get; set; }
        public TriggerCounter Counter { get; set; }

        // Returns the Basic output, constructing one from the legacy fields when a
        // trigger predates the expanded editor (so old triggers keep working at runtime).
        public TriggerOutput GetEffectiveBasic()
        {
            if (Basic != null)
            {
                return Basic;
            }
            return new TriggerOutput
            {
                DisplayTextEnabled = DisplayTextEnabled,
                DisplayText = DisplayText ?? string.Empty,
                AudioType = AudioTextEnabled ? TriggerAudioType.TextToSpeech : TriggerAudioType.None,
                TtsText = AudioText ?? string.Empty
            };
        }

        // the search field
        // may contain all regular expressions, and may also include simplified placeholder-style regular expressions
        //      Simple text
        //          Your spell fizzles!
        //      Simplified Placeholder-style Regular Expression:
        //          ^{backstabber} backstabs {target} for {damage} points of damage\.
        //      Normal Regular Expression:
        //          ^(?<backstabber>[\w` ]+) backstabs (?<target>[\w` ]+) for (?<damage>[\w` ]+) points of damage\.
        private string _SearchText = string.Empty;

        // Whether the pattern contains the {c} macro. Only {c} triggers depend on PlayerName, so
        // only they need their compiled regex invalidated when the logged-in player changes.
        // Computed once here (cold path) so the TriggerRegex hot path stays a cheap field check.
        private bool _hasContextToken;

        public string SearchText
        {
            get => _SearchText;
            set
            {
                if (_SearchText != value)
                {
                    _SearchText = value;
                    _TriggerRegex = null;
                    _hasContextToken = !string.IsNullOrEmpty(value) && _SearchText.IndexOf("{c}", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }

        // properties to support text to be displayed
        // may contain simplified placeholder-style regex
        //      Example:     Direction: {direction}
        public bool DisplayTextEnabled { get; set; }
        public string DisplayText { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ExpandedDisplayText => ExpandOutputText(DisplayText);

        // properties to support text to be spoken via TTS
        // may contain simplified placeholder-style regex
        //      Example:     Direction: {direction}
        public bool AudioTextEnabled { get; set; }
        public string AudioText { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ExpandedAudioText => ExpandOutputText(AudioText);

        // the regular expression for this trigger
        private Regex _TriggerRegex;

        private string _compiledContext;

        [Newtonsoft.Json.JsonIgnore]
        public Regex TriggerRegex
        {
            get
            {
                var context = PlayerName ?? string.Empty;
                if (_TriggerRegex != null && _hasContextToken && _compiledContext != context)
                {
                    _TriggerRegex = null;
                }

                // delay regex creation until its asked for
                if (_TriggerRegex == null && !string.IsNullOrWhiteSpace(_SearchText))
                {
                    // escape the PlayerName so any regex metacharacters in it (e.g. '.', '(') are
                    // treated literally and can't break (or throw on compiling) the trigger pattern
                    var escapedPlayerName = Regex.Escape(PlayerName ?? string.Empty);
                    var convertedSearchText = _SearchText.Replace("{c}", escapedPlayerName).Replace("{C}", escapedPlayerName);

                    var match = placeholderRegex.Match(convertedSearchText);
                    while (match.Success)
                    {
                        var group_name = match.Groups["xxx"].Value;
                        convertedSearchText = placeholderRegex.Replace(convertedSearchText, $"(?<{group_name}>[\\w` ]+)", 1);
                        match = match.NextMatch();
                    }

                    // now that we've converted the simplified regex to the real regex pattern, create and return the Regex object
                    _TriggerRegex = new Regex(convertedSearchText, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    _compiledContext = context;
                }

                return _TriggerRegex;
            }
        }

        // The user can define search patterns using a simplified regular expression syntax
        //       Example:    ^{backstabber} backstabs {target} for {damage} points of damage\.
        // The simplified form gets converted into the real regex expression for use in creating the TriggerRegex
        //       Example:    ^(?<backstabber>[\w` ]+) backstabs (?<target>[\w` ]+) for (?<damage>[\w` ]+) points of damage\.
        // Regular expression "named groups" from the user input are stored in a HashTable, with keys = the group names, and values = the parsed values
        //       Example:    Roger the Rogue backstabs a poor rabbit for 1000 points of damage
        // Resulting hashTable of (key, value) pairs:
        //          (backstabber, Roger the Rogue)
        //          (target, a poor rabbit)
        //          (damage, 1000)
        private readonly Hashtable valueHash = new Hashtable();     // list of named groups and their parsed values that we find in the search text

        // use this function to save the results after a regular expression search has been performed
        // if successful, then save the results into the trigger, for use later
        public void SaveNamedGroupValues(Match match)
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

        // Public expansion of simplified {name} placeholders using the values captured
        // from the last successful regex match. Used by all trigger outputs.
        public string Expand(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : ExpandOutputText(text);
        }

        // Tests a log line against this trigger, honoring the regex/plain-text setting.
        // On a regex match, captured named-group values are saved for output expansion.
        public bool Matches(string line)
        {
            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(SearchText))
            {
                return false;
            }

            if (EffectiveUseRegex)
            {
                var regex = TriggerRegex;
                if (regex == null)
                {
                    return false;
                }
                var match = regex.Match(line);
                if (match.Success)
                {
                    SaveNamedGroupValues(match);
                    return true;
                }
                return false;
            }

            return line.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }


        private string ExpandOutputText(string unExpandedText)
        {
            var rv = unExpandedText.Replace("{c}", PlayerName ?? string.Empty).Replace("{C}", PlayerName ?? string.Empty);

            // walk the list of matches, replacing the user match with the real match
            var match = placeholderRegex.Match(rv);
            while (match.Success)
            {
                // Handle match here...
                var group_name = match.Groups["xxx"].Value;

                // this key should be present, but confirm in case user made a typo
                if (valueHash.ContainsKey(group_name))
                {
                    // use regex to replace the placeholder named group with value from the hashtable
                    // do them one group at a time
                    var replace_text = $"{valueHash[group_name]}";
                    rv = placeholderRegex.Replace(rv, replace_text, 1);
                }

                match = match.NextMatch();
            }
            return rv;
        }
    }
}