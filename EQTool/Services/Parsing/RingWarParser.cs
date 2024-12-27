using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class RingWarParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public RingWarParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line == "Seneschal Aldikar shouts, TROOPS, TAKE YOUR POSITIONS!")
            {
                logEvents.Handle(new RingWarEvent { RoundNumber = 1, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            else if (line == "Narandi the Wretched shouts, 'Warriors! Charge through these pompous fools. Any you manage to capture shall become your personal slaves. The outlanders and the Seneschal must die! Bring me their heads!'")
            {
                logEvents.Handle(new RingWarEvent { RoundNumber = 2, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            else if (line == "Narandi the Wretched shouts, 'Veterans! Be sure that this time we allow none of the stumpymen to escape to create yet another city. This shall be our final war with these unworthy beings.'")
            {
                logEvents.Handle(new RingWarEvent { RoundNumber = 3, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }
    }
}
