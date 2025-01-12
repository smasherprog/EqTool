using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class RingWarParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public RingWarParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line == "Seneschal Aldikar shouts, TROOPS, TAKE YOUR POSITIONS!")
            {
                logEvents.Handle(new RingWarEvent { TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }
    }
}
