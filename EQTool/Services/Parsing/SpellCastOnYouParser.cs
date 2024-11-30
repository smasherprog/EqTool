using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class SpellCastOnYouParser : IEqLogParser
    {
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly EQSpells spells;
        private readonly SpellDurations spellDurations;
        private readonly DebugOutput debugOutput;

        private readonly List<string> IgnoreSpellsForGuesses = new List<string>(){
            "Tigir's Insects"
        };

        public SpellCastOnYouParser(SpellDurations spellDurations, LogEvents logEvents, ActivePlayer activePlayer, EQSpells spells, DebugOutput debugOutput)
        {
            this.debugOutput = debugOutput;
            this.spellDurations = spellDurations;
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
            this.spells = spells;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (spells.CastOnYouSpells.TryGetValue(line, out var foundspells))
            {
                foundspells = foundspells.Where(a => !IgnoreSpellsForGuesses.Contains(a.name)).ToList();
                var foundspell = spellDurations.MatchDragonRoar(foundspells, timestamp);
                if (foundspell != null)
                {
                    debugOutput.WriteLine($"{foundspell.name} Message: {line}", OutputType.Spells);
                    logEvents.Handle(new DragonRoarEvent
                    {
                        Spell = foundspell,
                        TimeStamp = timestamp,
                        Line = line,
                        LineCounter = lineCounter
                    });
                    return true;
                }
                foundspell = spellDurations.MatchClosestLevelToSpell(foundspells, timestamp);
                if (foundspell != null)
                {
                    debugOutput.WriteLine($"{foundspell.name} Message: {line}", OutputType.Spells);
                    logEvents.Handle(new SpellCastOnYouEvent
                    {
                        Spell = foundspell,
                        TimeStamp = timestamp,
                        Line = line,
                        LineCounter = lineCounter
                    });
                    return true;
                }
            }

            return false;
        }
    }
}