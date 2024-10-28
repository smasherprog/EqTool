using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class ExperienceGainedParser : IEqLogParseHandler
    {
        private readonly string ExpMessage = "You gain experience!!";
        private readonly LogEvents logEvents;

        public ExperienceGainedParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line == ExpMessage)
            {
                logEvents.Handle(new ExperienceGainedEvent
                {
                    Line = line,
                    LineCounter = lineCounter,
                    TimeStamp = timestamp
                });
                return true;
            }
            return false;
        }
    }
}
