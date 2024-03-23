using EQTool.Models;
using EQTool.ViewModels;
using System.Linq;

namespace EQTool.Services.Spells.Log
{
    public class SpellLogParse
    {
        private readonly ActivePlayer activePlayer;
        private readonly ParseHandleYouCasting parseHandleYouCasting;
        private readonly ParseSpellGuess parseSpellGuess;
        private readonly EQToolSettings settings;
        private readonly Spell HealSpell;

        public SpellLogParse(ParseSpellGuess parseSpellGuess, ParseHandleYouCasting parseHandleYouCasting, ActivePlayer activePlayer, EQToolSettings settings, EQSpells spells)
        {
            this.parseSpellGuess = parseSpellGuess;
            this.settings = settings;
            this.parseHandleYouCasting = parseHandleYouCasting;
            this.activePlayer = activePlayer;
            HealSpell = spells.AllSpells.FirstOrDefault(a => a.name == "Chloroplast") ?? spells.AllSpells.FirstOrDefault(a => a.name == "Regeneration");
        }

        public SpellParsingMatch MatchSpell(string message)
        {
            if (message == "You mend your wounds and heal some damage." || message == "You have failed to mend your wounds.")
            {
                return new SpellParsingMatch
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
                    TotalSecondsOverride = 6 * 60
                };
            }

            if (message.StartsWith(EQSpells.YouSpellisInterupted))
            {
                activePlayer.UserCastingSpell = null;
                return null;
            }
            if (message.StartsWith(EQSpells.YouBeginCasting))
            {
                parseHandleYouCasting.HandleYouBeginCastingSpellStart(message);
                return null;
            }
            else if (message.StartsWith(EQSpells.You))
            {
                if (activePlayer?.UserCastingSpell != null)
                {
                    if (message == activePlayer.UserCastingSpell.cast_on_you)
                    {
                        return parseHandleYouCasting.HandleYouBeginCastingSpellEnd(message);
                    }
                    else if (!string.IsNullOrWhiteSpace(activePlayer.UserCastingSpell.cast_on_other) && message.EndsWith(activePlayer.UserCastingSpell.cast_on_other))
                    {
                        return parseHandleYouCasting.HandleYouBeginCastingSpellOtherEnd(message);
                    }
                }

                return parseHandleYouCasting.HandleYouSpell(message);
            }

            if (message.StartsWith(EQSpells.Your))
            {
                if (activePlayer?.UserCastingSpell != null)
                {
                    if (message == activePlayer.UserCastingSpell.cast_on_you)
                    {
                        return parseHandleYouCasting.HandleYouBeginCastingSpellEnd(message);
                    }
                    else if (!string.IsNullOrWhiteSpace(activePlayer.UserCastingSpell.cast_on_other) && message.EndsWith(activePlayer.UserCastingSpell.cast_on_other))
                    {
                        return parseHandleYouCasting.HandleYouBeginCastingSpellOtherEnd(message);
                    }
                }

                var spell = parseHandleYouCasting.HandleYourSpell(message);
                if (spell != null)
                {
                    return spell;
                }

                return settings.BestGuessSpells ? parseSpellGuess.HandleBestGuessSpell(message) : null;
            }

            if (activePlayer?.UserCastingSpell != null)
            {
                if (message == activePlayer.UserCastingSpell.cast_on_you)
                {
                    return parseHandleYouCasting.HandleYouBeginCastingSpellEnd(message);
                }
                else if (!string.IsNullOrWhiteSpace(activePlayer.UserCastingSpell.cast_on_other) && message.EndsWith(activePlayer.UserCastingSpell.cast_on_other))
                {
                    return parseHandleYouCasting.HandleYouBeginCastingSpellOtherEnd(message);
                }
            }

            return settings.BestGuessSpells ? parseSpellGuess.HandleBestGuessSpell(message) : null;
        }
    }
}
