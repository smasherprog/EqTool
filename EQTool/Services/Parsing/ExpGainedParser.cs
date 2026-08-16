using EQTool.Models;
using System;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public class ExpGainedParser : IEqLogParser
    {
        //You gain experience!!
        //You gain party experience!!
        // https://regex101.com/r/kH3KND/1
        private const string expPattern = @"^You gain (party )?experience!!";
        private readonly Regex expRegex = new Regex(expPattern, RegexOptions.Compiled);

        private readonly LogEvents logEvents;

        public ExpGainedParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var expGainedEvent = Match(line, timestamp, lineCounter);
            if (expGainedEvent != null)
            {
                logEvents.Handle(expGainedEvent);
                return true;
            }
            return false;
        }

        public ExpGainedEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            ExpGainedEvent rv = null;

            var match = expRegex.Match(line);
            if (match.Success)
            {
                rv = new ExpGainedEvent
                {
                    TimeStamp = timestamp,
                    Line = line,
                    LineCounter = lineCounter
                };
            }

            return rv;
        }
    }
}
