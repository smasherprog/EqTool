using EQTool.Models;
using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace EQTool.Services.Parsing
{
    public class DeathParserNew : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public DeathParserNew(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            DeathEvent deathEvent = Match(line, timestamp);
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

            //[Thu Oct 17 20:03:17 2024].death is not online at this time.
            // simulate a player death
            if (line.StartsWith(".death "))
                rv = new DeathEvent(timestamp, line, "You");

            //[Mon Sep 16 14:32:02 2024] a Tesch Mas Gnoll has been slain by Genartik!
            //[Fri Nov 08 19:39:57 2019] You have been slain by a brigand!
            string slainByPattern = @"^(?<victim>[\w` ]+) (has|have) been slain by (?<killer>[\w` ]+)";
            var slainByRegex = new Regex(slainByPattern, RegexOptions.Compiled);
            var match = slainByRegex.Match(line);
            if (match.Success)
                rv = new DeathEvent(timestamp, line, match.Groups["victim"].Value, match.Groups["killer"].Value);

            //[Mon Sep 16 14:21:24 2024] You have slain a Tesch Mas Gnoll!
            string slainPattern = @"^You have slain (?<victim>[\w` ]+)";
            var slainRegex = new Regex(slainPattern, RegexOptions.Compiled);
            match = slainRegex.Match(line);
            if (match.Success)
                rv = new DeathEvent(timestamp, line, match.Groups["victim"].Value, "You");

            //[Sat Jan 16 20:12:37 2021] a bile golem died.
            //[Sat Apr 30 09:35:27 2022] Megachad died.
            //[Sat Apr 30 09:35:27 2022] You died.
            string diedPattern = @"^(?<victim>[\w` ]+) died\.$";
            var diedRegex = new Regex(diedPattern, RegexOptions.Compiled);
            match = diedRegex.Match(line);
            if (match.Success)
                rv = new DeathEvent(timestamp, line, match.Groups["victim"].Value);

            // return 
            return rv;
        }
    }
}
