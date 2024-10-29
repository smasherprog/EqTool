using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class LineParser
    {
        private readonly LogEvents logEvents;
        //this is special
        public LineParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            logEvents.Handle(new LineEvent
            {
                TimeStamp = timestamp,
                Line = line,
                LineCounter = lineCounter
            });
            return true;
        }

    }
}
