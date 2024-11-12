using EQTool.Models;
using System;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    //
    // this class will watch for Experience gained messages
    //
    public class ExpGainedParser : IEqLogParser
    {
        //You gain experience!!
        //You gain party experience!!
        // https://regex101.com/r/kH3KND/1
        private const string expPattern = @"^You gain (party )?experience!!";
        private readonly Regex expRegex = new Regex(expPattern, RegexOptions.Compiled);

        // class data
        private readonly LogEvents logEvents;

        //
        // ctor
        //
        public ExpGainedParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
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

        // parse this line to see if it contains what we are looking for
        // returns a CommsEvent object if a comms event is detecte, else
        // returns null
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
