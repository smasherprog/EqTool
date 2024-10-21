using EQTool.Models;
using System;
using System.Diagnostics;

namespace EQTool.Services.Parsing
{
    public class CampParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents; 
        private bool StillCamping = false;

        public CampParser(LogEvents logEvents)
        {
            this.logEvents = logEvents; 
        }

        public bool Handle(string line, DateTime timestamp)
        {
            return Camping(line, timestamp);
        }

        public bool Camping(string message, DateTime timestamp)
        {
            if (message == "It will take about 5 more seconds to prepare your camp.")
            {
                StillCamping = true;
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(1000 * 6);
                    if (StillCamping)
                    {
                        StillCamping = false;
                        Debug.WriteLine("CampEvent");
                        logEvents.Handle(new CampEvent { TimeStamp = timestamp });
                    }
                });
                return true;
            }
            else if (message == "You abandon your preparations to camp.")
            {
                StillCamping = false;
                return true;
            }
            return false;
        }

    }
}
