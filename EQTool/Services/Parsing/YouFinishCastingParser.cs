using EQTool.Models;
using EQTool.ViewModels;
using System;

namespace EQTool.Services.Parsing
{
    public class YouFinishCastingParser : IEqLogParser
    {
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly EQSpells spells;
        private readonly DebugOutput debugOutput;

        public YouFinishCastingParser(LogEvents logEvents, ActivePlayer activePlayer, EQSpells spells, DebugOutput debugOutput)
        {
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
            this.spells = spells;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var spell = activePlayer?.UserCastingSpell;
            if (spell != null)
            {
                if (line == spell.cast_on_you)
                {
                    debugOutput.WriteLine($"{spell.name} Message: {line}", OutputType.Spells);
                    logEvents.Handle(new YouFinishCastingEvent
                    {
                        Spell = spell,
                        TargetName = EQSpells.SpaceYou,
                        TimeStamp = timestamp,
                        Line = line,
                        LineCounter = lineCounter
                    });
                    return true;
                }
                else if (!string.IsNullOrWhiteSpace(spell.cast_on_other) && line.EndsWith(spell.cast_on_other))
                {
                    var targetname = line.Replace(spell.cast_on_other, string.Empty).Trim();
                    debugOutput.WriteLine($"{spell.name} Message: {line}", OutputType.Spells);
                    logEvents.Handle(new YouFinishCastingEvent
                    {
                        Spell = spell,
                        TargetName = targetname,
                        TimeStamp = timestamp,
                        Line = line,
                        LineCounter = lineCounter
                    });
                }
            }

            return false;
        }
    }
}