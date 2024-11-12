using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class FactionParser : IEqLogParser
    {
        private readonly string FactionStartMessage = "Your faction standing with";
        private readonly string FactionGotBetter = "got better.";
        private readonly string FactionGotWorse = "got worse.";
        private readonly string FactionCouldNotGetBetter = "could not possibly get any better.";
        private readonly string FactionCouldNotGetWorse = "could not possibly get any worse.";
        private readonly LogEvents logEvents;

        public FactionParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = FactionParse(line, timestamp, lineCounter);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }

        private FactionEvent FactionParse(string line, DateTime timestamp, int lineCounter)
        {
            if (line.StartsWith(FactionStartMessage))
            {
                var faction = line.Replace(FactionStartMessage, string.Empty)
                    .Replace(FactionGotBetter, string.Empty)
                    .Replace(FactionGotWorse, string.Empty)
                    .Replace(FactionCouldNotGetBetter, string.Empty)
                    .Replace(FactionCouldNotGetWorse, string.Empty)
                    .Trim();
                var factionstatus = FactionStatus.GotBetter;
                if (line.Contains(FactionGotWorse))
                {
                    factionstatus = FactionStatus.GotWorse;
                }
                else if (line.Contains(FactionCouldNotGetBetter))
                {
                    factionstatus = FactionStatus.CouldNotGetBetter;
                }
                else if (line.Contains(FactionCouldNotGetWorse))
                {
                    factionstatus = FactionStatus.CouldNotGetWorse;
                }
                return new FactionEvent { Faction = faction, FactionStatus = factionstatus, TimeStamp = timestamp, Line = line, LineCounter = lineCounter };
            }

            return null;
        }
    }
}
