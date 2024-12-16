using EQTool.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows.Shapes;

namespace EQTool.Services.Parsing
{
    public class SpellWornOffOtherParser : IEqLogParser
    {
        //https://regex101.com/r/5u7bs7/1
        // these references to the regex101.com website are very helpful, as that hash at the end of the URL reconstructs the entire test, with regex and test lines.  Somehow.  Magic...
        // So it's worth retaining to be able to go back and test later.
        private const string wornOffPattern = @"^Your (?<spell_name>[\w ]+) spell has worn off\.";
        private readonly Regex wornOffRegex = new Regex(wornOffPattern, RegexOptions.Compiled);

        private readonly EQSpells spells;
        private readonly LogEvents logEvents;
        private readonly DebugOutput debugOutput;

        public SpellWornOffOtherParser(EQSpells spells, LogEvents logEvents, DebugOutput debugOutput)
        {
            this.spells = spells;
            this.logEvents = logEvents;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            SpellWornOffOtherEvent e = MatchWornOffOther(line, timestamp, lineCounter);
            if (e != null)
            {
                debugOutput.WriteLine($"Spell: {e.SpellName}, Line: {line}", OutputType.Spells);
                logEvents.Handle(e);
                return true;
            }
            return false;
        }

        // check this line for a match of the desired search pattern
        public SpellWornOffOtherEvent MatchWornOffOther(string line, DateTime timestamp, int lineCounter)
        {
            // return value
            SpellWornOffOtherEvent rv = null;

            // check line for a match
            var match = wornOffRegex.Match(line);
            if (match.Success)
            {
                // return an Event of the correct benefit_detriment
                rv = new SpellWornOffOtherEvent
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
