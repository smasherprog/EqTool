using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services.Parsing
{
    public class LoadingPleaseWaitParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        // ctor
        public LoadingPleaseWaitParser(LogEvents logEvents)
        {
            this.logEvents = logEvents; 
        }

        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            bool rv = false;

            LoadingPleaseWaitEvent loadingPleaseWaitEvent = Match(line, timestamp, lineCounter);
            if (loadingPleaseWaitEvent != null)
            {
                logEvents.Handle(loadingPleaseWaitEvent);
                rv = true;
            }

            return rv;
        }

        // parse this line to see if it containns the search phrase
        public LoadingPleaseWaitEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            LoadingPleaseWaitEvent rv = null;
            if (line == "LOADING, PLEASE WAIT...")
            {
                rv = new LoadingPleaseWaitEvent
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
