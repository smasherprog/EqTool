using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EQTool.Services
{
    // class for simple non-timer Triggers
    public class UserDefinedTrigger
    {
        public int      TriggerID { get; set; }
        public bool     TriggerEnabled { get; set; }
        public string   TriggerName { get; set; }
        public bool     TextEnabled { get; set; }
        public string   DisplayText { get; set; }
        public bool     AudioEnabled { get; set; }
        public string   AudioText { get; set; }


        //
        // there are a couple of things going on here:
        //      - The user can define search patterns using a simplified regular expression syntax (similar to Gina)
        //              Example:    ^{backstabber} backstab(s)? {target} for {damage} points of damage\.
        //      - The simplified form gets converted into the real regex expression for internal use
        //              Example:    ^(?<backstabber>[\w` ]+) backstab(s)? (?<target>[\w` ]+) for (?<damage>[\w` ]+) points of damage\.
        //      - Regular expression "named groups" from the user input are stored in a HashTable, with keys = the group names, and values = the parsed values
        //              Example:    Roger the Rogue backstabs a poor rabbit for 1000 points of damage
        //              Resulting hashTable of (key, value) pairs:
        //                  backstabber, Roger the Rogue
        //                  target, a poor rabbit
        //                  damage, 1000
        //
        private string searchText;                                  // contains user-input simplified gina-style regex
        private string searchTextConverted;                         // contains actual regex
        private readonly Hashtable valueHash = new Hashtable();     // list of named groups and their parsed values that we find in the search text
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

                // regex to find gina-style simplified regex input, {some_name}
                const string gina_NamedGroup_pattern = @"\{(?<xxx>\w+)\}";

                try
                {
                    Regex regex = new Regex(gina_NamedGroup_pattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(250.0));
                    Match match = regex.Match(searchTextConverted);

                    // walk the list of matches, replacing the user match with the real match
                    while (match.Success)
                    {
                        // Handle match here...
                        string group_name = match.Groups["xxx"].Value;

                        // use regex to replace the gina named group with the real regex named group
                        // do them one group at a time
                        searchTextConverted = regex.Replace(searchTextConverted, $"(?<{group_name}>[\\w` ]+)", 1);

                        // at this point we know the key, but not the value, so add it to the list for later
                        valueHash.Add(group_name, "");

                        match = match.NextMatch();
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    // Do nothing: assume that exception represents no match.
                }
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
            Console.WriteLine(valueHash.Count);
            foreach (DictionaryEntry entry in valueHash)
            {
                Console.WriteLine($"key = {entry.Key}, value = {entry.Value}");
            }
        }
    }
}
