using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class YourSpellInterupptedParser : IEqLogParser
    {
        public const string YouSpellisInterupted = "Your spell is interrupted.";
        private readonly LogEvents logEvents;
        private readonly DebugOutput debugOutput;

        public YourSpellInterupptedParser(LogEvents logEvents, DebugOutput debugOutput)
        {
            this.logEvents = logEvents;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line == YouSpellisInterupted)
            {
                debugOutput.WriteLine($"Message: {line}", OutputType.Spells);
                logEvents.Handle(new YourSpellInterupptedEvent { Line = line, TimeStamp = timestamp, LineCounter = lineCounter });
                return true;
            }
            return false;
        }
    }
}
