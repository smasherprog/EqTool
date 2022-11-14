using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Diagnostics;

namespace EQTool.Services.Spells.Log
{
    public class SpellLogParse
    {
        private readonly ActivePlayer activePlayer;
        private readonly ParseHandleYouCasting parseHandleYouCasting;
        private readonly ParseSpellGuess parseSpellGuess;
        private readonly EQToolSettings settings;

        public SpellLogParse(ParseSpellGuess parseSpellGuess, ParseHandleYouCasting parseHandleYouCasting, ActivePlayer activePlayer, EQToolSettings settings)
        {
            this.parseSpellGuess = parseSpellGuess;
            this.settings = settings;
            this.parseHandleYouCasting = parseHandleYouCasting;
            this.activePlayer = activePlayer;
        }

        public SpellParsingMatch MatchSpell(string linelog)
        {
            var date = linelog.Substring(1, 25);
            if (DateTime.TryParse(date, out _))
            {

            }

            var message = linelog.Substring(27);
            Debug.WriteLine($"SpellParse: " + message);
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

                return parseHandleYouCasting.HandleYourSpell(message);
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
            else if (settings.BestGuessSpells)
            {
                return parseSpellGuess.HandleBestGuessSpell(message);
            }

            return null;
        }
    }
}
