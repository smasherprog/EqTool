using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class WelcomeParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        // ctor
        public WelcomeParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            bool rv = false;

            WelcomeEvent welcomeEvent = Match(line, timestamp, lineCounter);
            if (welcomeEvent != null)
            {
                logEvents.Handle(welcomeEvent);
                rv = true;
            }

            return rv;
        }

        // parse this line to see if it containns the search phrase
        public WelcomeEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            WelcomeEvent rv = null;
            if (line == "Welcome to EverQuest!")
            {
                rv = new WelcomeEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                };
            }
            return rv;
        }


    }
}
