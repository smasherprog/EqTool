using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EQTool.Services.Parsing
{

    //
    // DiscCoolDownParser
    //
    // Parse line for occurrences of the message received when player tries
    // to activate a discipline which is still on cooldown, and set a PigTimer
    // with the duration
    //
    public class DisciplineCooldownParser : IEqLogParser
    {
        private readonly ActivePlayer activePlayer;
        private readonly LogEvents logEvents;

        // https://regex101.com/r/vAkbvj/1
        // You can use the ability Puretone Discipline again in 48 minute(s) 45 seconds.
        private const string cooldownPattern = @"^You can use the ability (?<discname>[\w` ]+) again in (?<mm>[0-9]+) minute\(s\) (?<ss>[0-9]+) seconds.";
        private readonly Regex cooldownRegex = new Regex(cooldownPattern, RegexOptions.Compiled);

        //
        // ctor
        //
        public DisciplineCooldownParser(ActivePlayer activePlayer, LogEvents logEvents)
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
        }

        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var discEvent = Match(line, timestamp, lineCounter);
            if (discEvent != null)
            {
                logEvents.Handle(discEvent);
                return true;
            }
            return false;
        }

        // parse this line to see if it contains what we are looking for
        // returns a DisciplineCooldownEvent object if a disc cooldown log entry is detecte, else
        // returns null
        public DisciplineCooldownEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            DisciplineCooldownEvent rv = null;

            var match = cooldownRegex.Match(line);
            if (match.Success)
            {
                // extract data from the regex
                var mm = match.Groups["mm"].Value;
                var ss = match.Groups["ss"].Value;
                var discname = match.Groups["discname"].Value;

                // count up the seconds
                var timerSeconds = 0;
                if (ss != "")
                {
                    timerSeconds += int.Parse(ss);
                }
                if (mm != "")
                {
                    timerSeconds += 60 * int.Parse(mm);
                }

                // debugging tracking status message
                Console.WriteLine($"match found [{match}], mm = [{mm}], ss = [{ss}], discname = [{discname}], totalseconds = [{timerSeconds}]");

                // create Event instance for return
                rv = new DisciplineCooldownEvent
                {
                    Line = line,
                    LineCounter = lineCounter,
                    TimeStamp = timestamp,

                    TotalTimerSeconds = timerSeconds,
                    DisciplineName = discname
                };
            }

            return rv;
        }
    }
}