using EQTool.Models;
using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Shapes;

namespace EQTool.Services.Parsing
{
    public class LogDeathParse : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public LogDeathParse(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = GetDeadTarget(line);
            if (m != "")
            {
                logEvents.Handle(new DeadEvent { Name = m, TimeStamp = timestamp });
                return true;
            }
            return false;
        }

        public string GetDeadTarget(string line)
        {
            // return value
            string rv = "";

            //[Mon Sep 16 14:32:02 2024] a Tesch Mas Gnoll has been slain by Genartik!
            string slainByPattern = @"^(?<target>[\w ]+) has been slain by";
            var slainByRegex = new Regex(slainByPattern, RegexOptions.Compiled);
            var match = slainByRegex.Match(line);
            if (match.Success)
            {
                rv = match.Groups["target"].Value;
            }

            //[Mon Sep 16 14:21:24 2024] You have slain a Tesch Mas Gnoll!
            string slainPattern = @"^You have slain (?<target>[\w ]+)";
            var slainRegex = new Regex(slainPattern, RegexOptions.Compiled);
            match = slainRegex.Match(line);
            if (match.Success)
            {
                rv = match.Groups["target"].Value;
            }

            //[Sat Apr 30 09:35:27 2022] Megachad died.
            string diedPattern = @"^(?<target>[\w ]+) died\.$";
            var diedRegex = new Regex(diedPattern, RegexOptions.Compiled);
            match = diedRegex.Match(line);
            if (match.Success)
            {
                rv = match.Groups["target"].Value;
            }

            //[Mon Sep 16 14:21:24 2024] You have been slain
            // this regex allows the parser to watch for the real phrase, but also to be tested by
            // sending a tell while in-game to the non-existent user ".death"
            string playerDeathPattern = @"(^\.death )|(^You have been slain)";
            var playerDeathRegex = new Regex(playerDeathPattern, RegexOptions.Compiled);
            match = playerDeathRegex.Match(line);
            if (match.Success)
            {
                rv = "You";
            }

            // return 
            return rv;
        }
    }
}
