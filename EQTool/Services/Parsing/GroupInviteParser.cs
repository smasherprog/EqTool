using EQTool.Models;
using System;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class GroupInviteParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public GroupInviteParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = Parse(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                logEvents.Handle(new GroupInviteEvent { Inviter = m, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }

        public string Parse(string line)
        {
            if (line.EndsWith(" invites you to join a group."))
            {
                var remainder = line.Replace(" invites you to join a group.", string.Empty);
                var name = remainder.Trim();
                return name.Contains(' ') ? string.Empty : name;
            }

            return null;
        }
    }
}
