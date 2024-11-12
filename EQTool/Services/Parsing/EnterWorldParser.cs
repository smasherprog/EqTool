using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class EnterWorldParser : IEqLogParser
    {

        private readonly LogEvents logEvents;

        public EnterWorldParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = HasEnteredWorld(line);
            if (m)
            {
                logEvents.Handle(new EnteredWorldEvent { TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }

        public bool HasEnteredWorld(string line)
        {
            return line == "Welcome to EverQuest!";
        }
    }
}
