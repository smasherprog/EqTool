using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class MendWoundsParser : IEqLogParser
    { 
        private readonly LogEvents logEvents;

        public MendWoundsParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
         }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line == "You mend your wounds and heal some damage." || line == "You have failed to mend your wounds.")
            {
               this.logEvents.Handle(new MendWoundsEvent { Line = line, LineCounter = lineCounter, TimeStamp = timestamp });
            }
            return false;
        }
    }
}