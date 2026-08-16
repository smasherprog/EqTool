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

    public class DisciplineCooldownParser : IEqLogParser
    {
        private readonly ActivePlayer activePlayer;
        private readonly LogEvents logEvents;

        // https://regex101.com/r/vAkbvj/1
        // You can use the ability Puretone Discipline again in 48 minute(s) 45 seconds.
        private const string cooldownPattern = @"^You can use the ability (?<discname>[\w` ]+) again in (?<mm>[0-9]+) minute\(s\) (?<ss>[0-9]+) seconds.";
        private readonly Regex cooldownRegex = new Regex(cooldownPattern, RegexOptions.Compiled);

        public DisciplineCooldownParser(ActivePlayer activePlayer, LogEvents logEvents)
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
        }

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

        public DisciplineCooldownEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            DisciplineCooldownEvent rv = null;

            var match = cooldownRegex.Match(line);
            if (match.Success)
            {
                var mm = match.Groups["mm"].Value;
                var ss = match.Groups["ss"].Value;
                var discname = match.Groups["discname"].Value;

                var timerSeconds = 0;
                if (ss != "")
                {
                    timerSeconds += int.Parse(ss);
                }
                if (mm != "")
                {
                    timerSeconds += 60 * int.Parse(mm);
                }

                Console.WriteLine($"match found [{match}], mm = [{mm}], ss = [{ss}], discname = [{discname}], totalseconds = [{timerSeconds}]");

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
