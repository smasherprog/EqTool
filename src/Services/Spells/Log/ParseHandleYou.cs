using EQTool.Models;
using EQTool.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EQTool.Services.Spells.Log
{
    public class ParseHandleYouCasting
    {
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQSpells spells;

        public ParseHandleYouCasting(ActivePlayer activePlayer, IAppDispatcher appDispatcher, EQSpells spells)
        {
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.spells = spells;
        }

        public void HandleYouBeginCastingSpellStart(string message)
        {
            var spellname = message.Substring(EQSpells.YouBeginCasting.Length - 1).Trim().TrimEnd('.');
            if (spells.YouCastSpells.TryGetValue(spellname, out var foundspells))
            {
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player);
                Debug.WriteLine($"Self Casting Spell: {spellname} Delay: {foundspell.casttime}");
                appDispatcher.DispatchUI(() =>
                {
                    if (activePlayer.Player != null)
                    {
                        if (foundspell.Classes.Count == 1)
                        {
                            if (!activePlayer.Player.PlayerClass.HasValue)
                            {
                                activePlayer.Player.PlayerClass = foundspell.Classes.FirstOrDefault().Key;
                            }

                            if (activePlayer.Player.Level < foundspell.Classes.FirstOrDefault().Value)
                            {
                                activePlayer.Player.Level = foundspell.Classes.FirstOrDefault().Value;
                            }
                        }
                    }

                    activePlayer.UserCastingSpell = foundspell;
                    if (activePlayer.UserCastingSpell.casttime > 0)
                    {
                        activePlayer.UserCastingSpellCounter++;
                        _ = Task.Delay(activePlayer.UserCastingSpell.casttime * 4).ContinueWith(a =>
                        {
                            Debug.WriteLine($"Cleaning Spell");
                            appDispatcher.DispatchUI(() =>
                            {
                                if (--activePlayer.UserCastingSpellCounter <= 0)
                                {
                                    activePlayer.UserCastingSpellCounter = 0;
                                    activePlayer.UserCastingSpell = null;
                                }
                            });
                        });
                    }
                });
            }
        }

        public SpellParsingMatch HandleYouSpell(string message)
        {
            if (spells.CastOnYouSpells.TryGetValue(message, out var foundspells))
            {
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player);
                Debug.WriteLine($"You Casting Spell: {message} Delay: {foundspell.casttime}");
                return new SpellParsingMatch
                {
                    Spell = foundspell,
                    TargetName = EQSpells.SpaceYou
                };
            }

            return null;
        }

        public SpellParsingMatch HandleYourSpell(string message)
        {
            if (spells.CastOnYouSpells.TryGetValue(message, out var foundspells))
            {
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player);
                Debug.WriteLine($"Your Casting Spell: {message} Delay: {foundspell.casttime}");
                return new SpellParsingMatch
                {
                    Spell = foundspell,
                    TargetName = EQSpells.SpaceYou
                };
            }

            return null;
        }

        public SpellParsingMatch HandleYouBeginCastingSpellEnd(string message)
        {
            Debug.WriteLine($"Self Finished Spell: {message}");
            var spell = activePlayer.UserCastingSpell;
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.UserCastingSpell = null;
            });
            return new SpellParsingMatch
            {
                Spell = spell,
                TargetName = EQSpells.SpaceYou
            };
        }

        public SpellParsingMatch HandleYouBeginCastingSpellOtherEnd(string message)
        {
            var targetname = message.Replace(activePlayer.UserCastingSpell.cast_on_other, string.Empty).Trim();
            Debug.WriteLine($"Self Finished Spell: {message}");
            var spell = activePlayer.UserCastingSpell;
            return new SpellParsingMatch
            {
                Spell = spell,
                TargetName = targetname
            };
        }
    }
}
