using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class QuakeParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public QuakeParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = IsQuake(line);
            if (m)
            {
                logEvents.Handle(new QuakeEvent());
                return true;
            }
            return false;
        }

        public bool IsQuake(string line)
        {
            return line.Contains("You feel you should get somewhere safe as soon as possible");
        }
    }
}
