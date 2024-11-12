using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class QuakeParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public QuakeParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = IsQuake(line);
            if (m)
            {
                logEvents.Handle(new QuakeEvent { TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }

        private bool IsQuake(string line)
        {
            return line.Contains("You feel you should get somewhere safe as soon as possible");
        }
    }
}
