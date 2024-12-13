using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EQTool.Services.Parsing
{
    public class WornOffParser : IEqLogParser
    {
        //https://regex101.com/r/5u7bs7/1
        // these references to the regex101.com website are very helpful, as that hash at the end of the URL reconstructs the entire test, with regex and test lines.  Somehow.  Magic...
        // So it's worth retaining to be able to go back and test later.

        private const string wornOffPattern = @"^Your (?<spell_name>[\w ]+) spell has worn off\.";
        private readonly Regex wornOffRegex = new Regex(wornOffPattern, RegexOptions.Compiled);

        private readonly LogEvents logEvents;

        // ctor
        public WornOffParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        // impliment the interface Handle function
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = WornOffCheck(line, timestamp, lineCounter);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }

        // check this line for a match of the desired search pattern
        public WornOffEvent WornOffCheck(string line, DateTime timestamp, int lineCounter)
        {
            // return value
            WornOffEvent rv = null;

            // check line for a match
            var match = wornOffRegex.Match(line);
            if (match.Success)
            {
                // return an Event of the correct type
                rv = new WornOffEvent
                {
                    TimeStamp = timestamp,
                    Line = line,
                    LineCounter = lineCounter,
                    SpellName = match.Groups["spell_name"].Value
                };
            }

            return rv;
        }
    }
}
