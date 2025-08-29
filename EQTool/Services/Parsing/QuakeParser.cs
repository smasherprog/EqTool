using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class QuakeParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        public QuakeParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = IsQuake(line);
            if (m)
            {
                logEvents.Handle(new QuakeEvent { TimeStamp = timestamp, Line = line, LineCounter = lineCounter });
                return true;
            }
            return false;
        }

        private bool IsQuake(string line)
        {
            return line == "You feel you should get somewhere safe as soon as possible." ||
                line == "The gods have awoken to unleash their wrath across Norrath." ||
                line == "An unsettling silence smothers the land. Not a complete silence, but somehow quieter for it, the way a thick blanket of snow muffles the noise of the world. The chill of it pierces your bones, and you know, danger approaches." ||
                line == "You feel the need to get somewhere safe quickly.";
        }
    }
}
