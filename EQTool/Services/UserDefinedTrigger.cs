using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace EQTool.Services
{
    //
    // NOTE - All the functionality of this UserDefinedTrigger class has been moved
    // to the Trigger class.  The only purpose this class serves is to migrate any
    // old triggers in UserDefinedTriggers.txt, into the new Triggers class which
    // is now being saved with the rest of the EqTool info in settings.json.
    //

    // class for simple non-timer Triggers
    public class UserDefinedTrigger
    {

        // simple properties
        public int TriggerID { get; set; }
        public bool TriggerEnabled { get; set; }
        public string TriggerName { get; set; }

        // each trigger will have its own Regex instance, to prevent them from having to be rebuilt every time
        // the regex will be configured on the call to SearchText.set()
        private Regex triggerRegex;
        public Regex TriggerRegex { get { return triggerRegex; } }

        // set up a regex to search for gina-style regex entries, for use in SearchText and DisplayText and AudioText
        // finds simplified regex patterns of {some_name}
        private const string ginaRegexPattern = @"\{(?<xxx>\w+)\}";
        private readonly Regex ginaRegex = new Regex(ginaRegexPattern, RegexOptions.Compiled);

        //
        // there are a couple of things going on here:
        //      - The user can define search patterns using a simplified regular expression syntax (similar to Gina)
        //              Example:    ^{backstabber} backstabs {target} for {damage} points of damage\.
        //      - The simplified form gets converted into the real regex expression for internal use
        //              Example:    ^(?<backstabber>[\w` ]+) backstabs (?<target>[\w` ]+) for (?<damage>[\w` ]+) points of damage\.
        //      - Regular expression "named groups" from the user input are stored in a HashTable, with keys = the group names, and values = the parsed values
        //              Example:    Roger the Rogue backstabs a poor rabbit for 1000 points of damage
        //              Resulting hashTable of (key, value) pairs:
        //                  (backstabber, Roger the Rogue)
        //                  (target, a poor rabbit)
        //                  (damage, 1000)
        //
        private readonly Hashtable valueHash = new Hashtable();     // list of named groups and their parsed values that we find in the search text
        private string searchText;                                  // contains user-input simplified gina-style regex
        private string searchTextConverted;                         // contains actual regex
        public string SearchText
        {
            get
            {
                // return the converted-to-real-regex version
                return searchTextConverted;
            }
            set
            {
                // set both searchText fields whenver the set method is called
                searchText = value;
                searchTextConverted = value;

                //
                // convert search text user input which may contain simplified gina-style regex, i.e.
                //      {some_name}
                // to the real regex match input, i.e.
                //      (?<some_name>[\w` ]+)
                //
                // note the regex allows the field to contain
                //      - \w for any character found in words
                //      - spaces, to capture multi-word targets, i.e. "Spider Queen D`Zee"
                //      - the ` back tick, which actually appears in quite a few mob names


                // walk the list of matches, replacing the user match with the real match
                Match match = ginaRegex.Match(searchTextConverted);
                while (match.Success)
                {
                    // Handle match here...
                    string group_name = match.Groups["xxx"].Value;

                    // use regex to replace the gina named group with the real regex named group
                    // do them one group at a time
                    searchTextConverted = ginaRegex.Replace(searchTextConverted, $"(?<{group_name}>[\\w` ]+)", 1);

                    // at this point we know the key, but not the value, so add it to the list for later
                    valueHash.Add(group_name, "");

                    match = match.NextMatch();
                }

                // confirm we have all the keys and values
                Console.WriteLine($"Trigger ID = {TriggerID}, Trigger Name = {TriggerName}");
                foreach (DictionaryEntry entry in valueHash)
                {
                    Console.WriteLine($"    key = {entry.Key}, value = {entry.Value}");
                }

                // set up the regex for this trigger
                triggerRegex = new Regex(searchTextConverted);
            }
        }

        //
        // use this function to save the results after a regular expression search has been performed
        // if successful, then save the results into the trigger, for use later
        internal void SaveNamedGroupValues(Match match)
        {
            // walk the Groups list in the regex Match, and save the values we care about
            foreach (Group g in match.Groups)
            {
                // the keys got loaded during the conversion process of SearchText.set()
                if (valueHash.ContainsKey(g.Name))
                {
                    valueHash[g.Name] = g.Value;
                }
            }

            // confirm we have all the keys and values
            Console.WriteLine($"Trigger ID = {TriggerID}, Trigger Name = {TriggerName}");
            foreach (DictionaryEntry entry in valueHash)
            {
                Console.WriteLine($"    key = {entry.Key}, value = {entry.Value}");
            }
        }

        //
        // custom getter for DisplayText
        //
        public bool TextEnabled { get; set; }

        private string displayText;
        public string DisplayText
        {
            set { displayText = value; }
            get { return ProcessOutputText(displayText); }
        }

        //
        // custom getter for AudioText
        //
        public bool AudioEnabled { get; set; }

        private string audioText;


        public string AudioText
        {
            set { audioText = value; }
            get { return ProcessOutputText(audioText); }
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
