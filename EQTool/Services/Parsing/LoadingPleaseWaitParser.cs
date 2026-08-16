using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class LoadingPleaseWaitParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public LoadingPleaseWaitParser(LogEvents logEvents)
        {
            this.logEvents = logEvents; 
        }

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
