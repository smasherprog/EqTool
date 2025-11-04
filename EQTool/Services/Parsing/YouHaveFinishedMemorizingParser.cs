using EQTool.Models;
using System;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class YouHaveFinishedMemorizingParser : IEqLogParser
    {
        public const string Youhavefinishedmemorizing = "You have finished memorizing ";
        private readonly LogEvents logEvents;
        private readonly DebugOutput debugOutput;
        private readonly EQSpells spells;

        public YouHaveFinishedMemorizingParser(LogEvents logEvents, DebugOutput debugOutput, EQSpells spells)
        {
            this.spells = spells;
            this.logEvents = logEvents;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line.StartsWith(Youhavefinishedmemorizing))
            {
                var spell = line.Replace(Youhavefinishedmemorizing, string.Empty).Trim(' ', '.');
                debugOutput.WriteLine($"Message: {line}", OutputType.Spells);
                logEvents.Handle(new YouHaveFinishedMemorizingEvent { SpellName = spell, Line = line, TimeStamp = timestamp, LineCounter = lineCounter });
                spells.AllSpells.TryGetValue(spell, out var foundspell);
                if (foundspell != null && foundspell.Classes.Count == 1)
                {
                    logEvents.Handle(new ClassDetectedEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line, PlayerClass = foundspell.Classes.FirstOrDefault().Key });
                    logEvents.Handle(new PlayerLevelDetectionEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line, PlayerLevel = foundspell.Classes.FirstOrDefault().Value });
                }

                return true;
            }
            return false;
        }
    }
}
