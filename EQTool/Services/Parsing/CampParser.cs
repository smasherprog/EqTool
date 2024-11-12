using EQTool.Models;
using System;
using System.Diagnostics;

namespace EQTool.Services.Parsing
{
    public class CampParser : IEqLogParser
    {
        private readonly LogEvents logEvents;
        private bool StillCamping = false;

        public CampParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line == "It will take about 5 more seconds to prepare your camp.")
            {
                StillCamping = true;
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(1000 * 6);
                    if (StillCamping)
                    {
                        StillCamping = false;
                        Debug.WriteLine("CampEvent");
                        logEvents.Handle(new CampEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line });
                    }
                });
                return true;
            }
            else if (line == "You abandon your preparations to camp.")
            {
                StillCamping = false;
                return true;
            }
            return false;
        }

    }
}
