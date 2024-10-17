using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class FailedFeignParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public FailedFeignParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            var m = FailedFaignCheck(line);
            if (!string.IsNullOrWhiteSpace(m))
            {
                logEvents.Handle(new FailedFeignEvent { PersonWhoFailedFeign = m, TimeStamp = timestamp });
                return true;
            }
            return false;
        }

        public string FailedFaignCheck(string line)
        {
            var indexof = line.IndexOf(" has fallen to the ground.");
            return indexof != -1 ? line.Substring(0, indexof) : string.Empty;
        }
    }
}
