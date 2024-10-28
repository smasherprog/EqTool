using EQTool.Models;
using System;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public class DeathParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;
        private const string slainByPattern = @"^(?<victim>[\w` ]+) (has|have) been slain by (?<killer>[\w` ]+)";
        private const string slainPattern = @"^You have slain (?<victim>[\w` ]+)";
        private const string diedPattern = @"^(?<victim>[\w` ]+) died\.$";

        private readonly Regex diedRegex = new Regex(diedPattern, RegexOptions.Compiled);
        private readonly Regex slainRegex = new Regex(slainPattern, RegexOptions.Compiled);
        private readonly Regex slainByRegex = new Regex(slainByPattern, RegexOptions.Compiled);

        public DeathParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var deathEvent = Match(line, timestamp, lineCounter);
            if (deathEvent != null)
            {
                deathEvent.Line = line;
                deathEvent.TimeStamp = timestamp;
                deathEvent.LineCounter = lineCounter;
                logEvents.Handle(deathEvent);
                return true;
            }
            return false;
        }

        // function to check for a death event
        public DeathEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            //[Mon Sep 16 14:32:02 2024] a Tesch Mas Gnoll has been slain by Genartik!
            //[Fri Nov 08 19:39:57 2019] You have been slain by a brigand! 
            var match = slainByRegex.Match(line);
            if (match.Success)
            {
                return new DeathEvent
                {
                    Killer = match.Groups["killer"].Value,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    Line = line,
                    Victim = match.Groups["victim"].Value
                };
            }

            //[Mon Sep 16 14:21:24 2024] You have slain a Tesch Mas Gnoll!

            match = slainRegex.Match(line);
            if (match.Success)
            {
                return new DeathEvent
                {
                    Killer = "You",
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    Line = line,
                    Victim = match.Groups["victim"].Value
                };
            }

            //[Sat Jan 16 20:12:37 2021] a bile golem died.
            //[Sat Apr 30 09:35:27 2022] Megachad died.
            //[Sat Apr 30 09:35:27 2022] You died.
            match = diedRegex.Match(line);
            if (match.Success)
            {
                return new DeathEvent
                {
                    Killer = string.Empty,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    Line = line,
                    Victim = match.Groups["victim"].Value
                };
            }

            // return 
            return null;
        }
    }
}
