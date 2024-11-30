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
            var userCastingSpell = activePlayer.UserCastingSpell;
            var userCastSpellDateTime = activePlayer.UserCastSpellDateTime;
            if (userCastingSpell != null && userCastSpellDateTime != null)
            {
                var dt = timestamp - userCastSpellDateTime.Value;
                if (dt.TotalMilliseconds >= (userCastingSpell.casttime - 300))
                {
                    if (line == userCastingSpell.cast_on_you)
                    {
                        debugOutput.WriteLine($"{userCastingSpell.name} Message: {line}", OutputType.Spells);
                        logEvents.Handle(new YouFinishCastingEvent
                        {
                            Spell = userCastingSpell,
                            TargetName = EQSpells.SpaceYou,
                            TimeStamp = timestamp,
                            Line = line,
                            LineCounter = lineCounter
                        });
                        return true;
                    }
                    else if (!string.IsNullOrWhiteSpace(userCastingSpell.cast_on_other) && line.EndsWith(userCastingSpell.cast_on_other))
                    {
                        var targetname = line.Replace(userCastingSpell.cast_on_other, string.Empty).Trim();
                        debugOutput.WriteLine($"{userCastingSpell.name} Message: {line}", OutputType.Spells);
                        logEvents.Handle(new YouFinishCastingEvent
                        {
                            Spell = userCastingSpell,
                            TargetName = targetname,
                            TimeStamp = timestamp,
                            Line = line,
                            LineCounter = lineCounter
                        });
                        return true;
                    }
                }
            }

            return false;
        }
    }
}