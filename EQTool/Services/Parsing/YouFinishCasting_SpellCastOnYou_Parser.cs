using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public class YouFinishCasting_SpellCastOnYou_Parser : IEqLogParser
    {
        private const string protectedPattern = @"^You try to cast a spell on (?<target_name>[\w ]+)\, but they are protected\.";
        private readonly Regex protectedRegex = new Regex(protectedPattern, RegexOptions.Compiled);
        
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly EQSpells spells;
        private readonly SpellDurations spellDurations;
        private readonly DebugOutput debugOutput;

        private readonly List<string> IgnoreSpellsForGuesses = new List<string>(){
            "Tigir's Insects"
        };

        public YouFinishCasting_SpellCastOnYou_Parser(SpellDurations spellDurations, LogEvents logEvents, ActivePlayer activePlayer, EQSpells spells, DebugOutput debugOutput)
        {
            this.debugOutput = debugOutput;
            this.spellDurations = spellDurations;
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
            this.spells = spells;
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
                    else if (userCastingSpell.name == "Theft of Thought" && line == "Your target has no mana to affect")
                    {
                        debugOutput.WriteLine($"{userCastingSpell.name} Message: {line}", OutputType.Spells);
                        logEvents.Handle(new YourSpellInterruptedEvent
                        {
                            TimeStamp = timestamp,
                            Line = line,
                            LineCounter = lineCounter
                        });
                        return true;
                    }
                    
                    var match = protectedRegex.Match(line);
                    if (match.Success)
                    {
                        debugOutput.WriteLine($"{userCastingSpell.name} Message: {line}", OutputType.Spells);
                        logEvents.Handle(new ResistSpellEvent
                        {
                            Spell = userCastingSpell,
                            isYou = false,
                            TimeStamp = timestamp,
                            Line = line,
                            LineCounter = lineCounter
                        });
                        return true;
                    }
                }
            }

            if (line.EndsWith(".."))
            {
                line = line.Substring(0, line.Length - 1);
            }

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