using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class YourSpellInterupptedParser : IEqLogParser
    { 
        public const string YouSpellisInterupted = "Your spell is interrupted.";
        private readonly LogEvents logEvents;

        public YourSpellInterupptedParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        { 
            if (line == YouSpellisInterupted)
            {
                logEvents.Handle(new YourSpellInterupptedEvent { Line = line, TimeStamp = timestamp, LineCounter = lineCounter });
                return true;
            }
            return false;
        } 
    }
}
