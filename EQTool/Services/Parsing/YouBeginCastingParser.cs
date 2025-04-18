﻿using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class YouBeginCastingParser : IEqLogParser
    {
        public const string YouBeginCasting = "You begin casting ";
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly EQSpells spells;
        private readonly SpellDurations spellDurations;
        private readonly DebugOutput debugOutput;

        public YouBeginCastingParser(SpellDurations spellDurations, LogEvents logEvents, ActivePlayer activePlayer, EQSpells spells, DebugOutput debugOutput)
        {
            this.spellDurations = spellDurations;
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
            this.spells = spells;
            this.debugOutput = debugOutput;
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            if (line.StartsWith(YouBeginCasting))
            {
                var spellname = line.Substring(YouBeginCasting.Length - 1).Trim().TrimEnd('.');
                if (spells.YouCastSpells.TryGetValue(spellname, out var foundspells))
                {
                    var foundspell = spellDurations.MatchClosestLevelToSpell(foundspells, timestamp);
                    debugOutput.WriteLine($"{foundspell.name} Message: {line}", OutputType.Spells);
                    if (foundspell.Classes.Count == 1)
                    {
                        if (activePlayer.Player.PlayerClass == EQToolShared.Enums.PlayerClasses.Enchanter &&
                            foundspell.Classes.ContainsKey(EQToolShared.Enums.PlayerClasses.Enchanter))
                        {
                            logEvents.Handle(new PlayerLevelDetectionEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line, PlayerLevel = foundspell.Classes.FirstOrDefault().Value });
                        }
                        else
                        {
                            if (foundspell.DescrNumber != DescrNumber.IllusionPlayer && foundspell.DescrNumber != DescrNumber.IllusionOther)
                            {
                                logEvents.Handle(new ClassDetectedEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line, PlayerClass = foundspell.Classes.FirstOrDefault().Key });
                            }

                            logEvents.Handle(new PlayerLevelDetectionEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = line, PlayerLevel = foundspell.Classes.FirstOrDefault().Value });
                        }
                    }
                    logEvents.Handle(new YouBeginCastingEvent { Line = line, LineCounter = lineCounter, TimeStamp = timestamp, Spell = foundspell });
                    return true;
                }
            }

            return false;
        }
    }
}
