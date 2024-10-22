using EQTool.Models;
using System;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public class DeathParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public DeathParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var deathEvent = Match(line, timestamp);
            if (deathEvent != null)
            {
                logEvents.Handle(deathEvent);
                return true;
            }
            return false;
        }

        // function to check for a death event
        public DeathEvent Match(string line, DateTime timestamp)
        {
            // return value
            DeathEvent rv = null;

            //[Mon Sep 16 14:32:02 2024] a Tesch Mas Gnoll has been slain by Genartik!
            //[Fri Nov 08 19:39:57 2019] You have been slain by a brigand!
            var slainByPattern = @"^(?<victim>[\w` ]+) (has|have) been slain by (?<killer>[\w` ]+)";
            var slainByRegex = new Regex(slainByPattern, RegexOptions.Compiled);
            var match = slainByRegex.Match(line);
            if (match.Success)
            {
                rv = new DeathEvent(timestamp, line, match.Groups["victim"].Value, match.Groups["killer"].Value);
            }

            //[Mon Sep 16 14:21:24 2024] You have slain a Tesch Mas Gnoll!
            var slainPattern = @"^You have slain (?<victim>[\w` ]+)";
            var slainRegex = new Regex(slainPattern, RegexOptions.Compiled);
            match = slainRegex.Match(line);
            if (match.Success)
            {
                rv = new DeathEvent(timestamp, line, match.Groups["victim"].Value, "You");
            }

            //[Sat Jan 16 20:12:37 2021] a bile golem died.
            //[Sat Apr 30 09:35:27 2022] Megachad died.
            //[Sat Apr 30 09:35:27 2022] You died.
            var diedPattern = @"^(?<victim>[\w` ]+) died\.$";
            var diedRegex = new Regex(diedPattern, RegexOptions.Compiled);
            match = diedRegex.Match(line);
            if (match.Success)
            {
                rv = new DeathEvent(timestamp, line, match.Groups["victim"].Value);
            }

            // return 
            return rv;
        }
    }
}
