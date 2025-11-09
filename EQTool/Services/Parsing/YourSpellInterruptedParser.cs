using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class YourSpellInterruptedParser : IEqLogParser
    {
        public const string YouSpellisInterrupted = "Your spell is interrupted.";
        private readonly LogEvents logEvents;
        private readonly DebugOutput debugOutput;

        public YourSpellInterruptedParser(LogEvents logEvents, DebugOutput debugOutput)
        {
            this.logEvents = logEvents;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line == YouSpellisInterrupted)
            {
                debugOutput.WriteLine($"Message: {line}", OutputType.Spells);
                logEvents.Handle(new YourSpellInterruptedEvent { Line = line, TimeStamp = timestamp, LineCounter = lineCounter });
                return true;
            }
            return false;
        }
    }
}
