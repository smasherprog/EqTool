using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Diagnostics;

namespace EQTool.Services.Parsing
{
    public class YouFinishCastingParser : IEqLogParser
    { 
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly EQSpells spells;

        public YouFinishCastingParser(LogEvents logEvents, ActivePlayer activePlayer, EQSpells spells)
        {
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
            this.spells = spells;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (activePlayer.UserCastingSpell != null)
            {
                if (line == activePlayer.UserCastingSpell.cast_on_you)
                {
                    Debug.WriteLine($"Self Finished Spell: {line}");
                    var spell = activePlayer.UserCastingSpell;
                    this.logEvents.Handle(new YouFinishCastingEvent
                    {
                        Spell = spell,
                        TargetName = EQSpells.SpaceYou,
                        TimeStamp = timestamp,
                        Line = line,
                        LineCounter = lineCounter
                    });
                    return true;
                }
                else if (!string.IsNullOrWhiteSpace(activePlayer.UserCastingSpell.cast_on_other) && line.EndsWith(activePlayer.UserCastingSpell.cast_on_other))
                {
                    var targetname = line.Replace(activePlayer.UserCastingSpell.cast_on_other, string.Empty).Trim();
                    Debug.WriteLine($"Self Finished Spell: {line}");
                    var spell = activePlayer.UserCastingSpell;
                    this.logEvents.Handle(new YouFinishCastingEvent
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