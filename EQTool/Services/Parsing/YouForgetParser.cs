using EQTool.Models;
using System;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class YouForgetParser : IEqLogParser
    {
        public const string youForget = "You forget ";
        
        private readonly LogEvents logEvents;
        private readonly DebugOutput debugOutput;
        private readonly EQSpells spells;

        public YouForgetParser(LogEvents logEvents, DebugOutput debugOutput, EQSpells spells)
        {
            this.spells = spells;
            this.logEvents = logEvents;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (!line.StartsWith(youForget))
                return false;

            var spell = line.Replace(youForget, string.Empty).Trim(' ', '.');
            debugOutput.WriteLine($"Message: {line}", OutputType.Spells);
            logEvents.Handle(new YouForgetEvent { SpellName = spell, Line = line, TimeStamp = timestamp, LineCounter = lineCounter });
            
            if (spells.AllSpells.TryGetValue(spell, out var foundSpell) && foundSpell.Classes.Count == 1)
            {
                logEvents.Handle(new ClassDetectedEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line, PlayerClass = foundSpell.Classes.FirstOrDefault().Key });
                logEvents.Handle(new PlayerLevelDetectionEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line, PlayerLevel = foundSpell.Classes.FirstOrDefault().Value });
            }

            return true;
        }
    }
}
