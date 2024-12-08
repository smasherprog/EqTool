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
            else if (line == "Seneschal Aldikar shouts, 'Enough! Show yourself coward! Your blasphemous words shall be etched upon your spacious brow. All will mock you for generations to come. Your own god will forsake you when he witnesses your defeat here today!'")
            {
                logEvents.Handle(new RingWarEvent { RoundNumber = 2, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            else if (line == "Narandi the Wretched shouts, 'Enough chatter. Our veterans approach now to finish you. You have been tested and your weaknesses have been assessed. Bid farewell to your dear Thurgadin, those of you who are fortunate enough to survive the slaughter shall make a new home in the Kromrif slave pens!'")
            {
                logEvents.Handle(new RingWarEvent { RoundNumber = 3, TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }

        private bool IsQuake(string line)
        {
            return line.Contains("You feel you should get somewhere safe as soon as possible");
        }
    }
}
