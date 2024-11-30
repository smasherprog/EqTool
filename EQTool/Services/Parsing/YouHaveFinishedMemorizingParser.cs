using EQTool.Models;
using System;

namespace EQTool.Services.Parsing
{
    public class YouHaveFinishedMemorizingParser : IEqLogParser
    {
        public const string Youhavefinishedmemorizing = "You have finished memorizing ";
        private readonly LogEvents logEvents;
        private readonly DebugOutput debugOutput;

        public YouHaveFinishedMemorizingParser(LogEvents logEvents, DebugOutput debugOutput)
        {
            this.logEvents = logEvents;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line.StartsWith(Youhavefinishedmemorizing))
            {
                var spell = line.Replace(Youhavefinishedmemorizing, string.Empty);
                debugOutput.WriteLine($"Message: {line}", OutputType.Spells);
                logEvents.Handle(new YouHaveFinishedMemorizingEvent { SpellName = spell, Line = line, TimeStamp = timestamp, LineCounter = lineCounter });
                return true;
            }
            return false;
        }
    }
}
