using EQTool.Models;
using EQTool.Services.Parsing.Helpers;
using EQTool.ViewModels;
using System;
using System.Linq;

namespace EQTool.Services.Parsing
{
    public class SpellCastParser : IEqLogParser
    {
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly ParseHandleYouCasting parseHandleYouCasting;
        private readonly ParseSpellGuess parseSpellGuess;
        private readonly EQToolSettings settings;
        private readonly Spell HealSpell;

        public SpellCastParser(LogEvents logEvents, ParseSpellGuess parseSpellGuess, ParseHandleYouCasting parseHandleYouCasting, ActivePlayer activePlayer, EQToolSettings settings, EQSpells spells)
        {
            this.logEvents = logEvents;
            this.parseSpellGuess = parseSpellGuess;
            this.settings = settings;
            this.parseHandleYouCasting = parseHandleYouCasting;
            this.activePlayer = activePlayer;
            HealSpell = spells.AllSpells.FirstOrDefault(a => a.name == "Chloroplast") ?? spells.AllSpells.FirstOrDefault(a => a.name == "Regeneration");
        }

        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var m = MatchSpell(line, timestamp, lineCounter);
            if (m != null)
            {
                logEvents.Handle(m);
                return true;
            }
            return false;
        }


        public SpellCastEvent MatchSpell(string line, DateTime timestamp, int lineCounter)
        {
            if (line == "You mend your wounds and heal some damage." || line == "You have failed to mend your wounds.")
            {
                return new SpellCastEvent
                {
                    MultipleMatchesFound = false,
                    Spell = new Spell
                    {
                        buffduration = HealSpell.buffduration,
                        buffdurationformula = HealSpell.buffduration,
                        casttime = HealSpell.casttime,
                        cast_on_other = HealSpell.cast_on_other,
                        cast_on_you = HealSpell.cast_on_you,
                        Classes = new System.Collections.Generic.Dictionary<EQToolShared.Enums.PlayerClasses, int>() { { EQToolShared.Enums.PlayerClasses.Monk, 1 } },
                        DescrNumber = HealSpell.DescrNumber,
                        id = HealSpell.id,
                        name = "Mend",
                        pvp_buffdurationformula = HealSpell.pvp_buffdurationformula,
                        Rect = HealSpell.Rect,
                        ResistCheck = HealSpell.ResistCheck,
                        resisttype = HealSpell.resisttype,
                        SpellIcon = HealSpell.SpellIcon,
                        SpellType = HealSpell.SpellType,
                        spell_fades = HealSpell.spell_fades,
                        spell_icon = HealSpell.spell_icon,
                        type = HealSpell.type
                    },
                    TargetName = EQSpells.SpaceYou,
                    TotalSecondsOverride = 6 * 60,
                    TimeStamp = timestamp,
                    Line = line,
                    CastByYou = false,
                    LineCounter = lineCounter
                };
            }

            if (line.StartsWith(EQSpells.YouSpellisInterupted))
            {
                activePlayer.UserCastingSpell = null;
                return null;
            }
            if (line.StartsWith(EQSpells.YouBeginCasting))
            {
                parseHandleYouCasting.HandleYouBeginCastingSpellStart(line, timestamp, lineCounter);

                // fire off an event indicating spell casting has started
                logEvents.Handle(new YouBeginCastingEvent { Line = line, LineCounter = lineCounter, TimeStamp = timestamp });
                // should probably return true?  I am hesitant to change, not sure what else might break if I do
                //return true;
                return null;
            }
            else if (line.StartsWith(EQSpells.You))
            {
                if (activePlayer?.UserCastingSpell != null)
                {
                    if (line == activePlayer.UserCastingSpell.cast_on_you)
                    {
                        return parseHandleYouCasting.HandleYouBeginCastingSpellEnd(line, timestamp, lineCounter);
                    }
                    else if (!string.IsNullOrWhiteSpace(activePlayer.UserCastingSpell.cast_on_other) && line.EndsWith(activePlayer.UserCastingSpell.cast_on_other))
                    {
                        return parseHandleYouCasting.HandleYouBeginCastingSpellOtherEnd(line, timestamp, lineCounter);
                    }
                }

                return parseHandleYouCasting.HandleYouSpell(line, timestamp, lineCounter);
            }

            if (line.StartsWith(EQSpells.Your))
            {
                if (activePlayer?.UserCastingSpell != null)
                {
                    if (line == activePlayer.UserCastingSpell.cast_on_you)
                    {
                        return parseHandleYouCasting.HandleYouBeginCastingSpellEnd(line, timestamp, lineCounter);
                    }
                    else if (!string.IsNullOrWhiteSpace(activePlayer.UserCastingSpell.cast_on_other) && line.EndsWith(activePlayer.UserCastingSpell.cast_on_other))
                    {
                        return parseHandleYouCasting.HandleYouBeginCastingSpellOtherEnd(line, timestamp, lineCounter);
                    }
                }

                var spell = parseHandleYouCasting.HandleYourSpell(line, timestamp, lineCounter);
                return spell ?? (settings.BestGuessSpells ? parseSpellGuess.HandleBestGuessSpell(line, timestamp, lineCounter) : null);
            }

            if (activePlayer?.UserCastingSpell != null)
            {
                if (line == activePlayer.UserCastingSpell.cast_on_you)
                {
                    return parseHandleYouCasting.HandleYouBeginCastingSpellEnd(line, timestamp, lineCounter);
                }
                else if (!string.IsNullOrWhiteSpace(activePlayer.UserCastingSpell.cast_on_other) && line.EndsWith(activePlayer.UserCastingSpell.cast_on_other))
                {
                    return parseHandleYouCasting.HandleYouBeginCastingSpellOtherEnd(line, timestamp, lineCounter);
                }
            }

            return settings.BestGuessSpells ? parseSpellGuess.HandleBestGuessSpell(line, timestamp, lineCounter) : null;
        }
    }
}
