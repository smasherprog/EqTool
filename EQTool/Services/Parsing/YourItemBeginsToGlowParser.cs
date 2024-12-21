using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class YourItemBeginsToGlowParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public YourItemBeginsToGlowParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = Parse(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                logEvents.Handle(new YourItemBeginsToGlow { ItemName = m, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }

        private string Parse(string line)
        {
            if (line.StartsWith(EQSpells.Your) && line.EndsWith(" begins to glow."))
            {
                return line.Substring(EQSpells.Your.Length).Replace(" begins to glow.", string.Empty).Trim();
            }
            return string.Empty;
        }
    }
}
