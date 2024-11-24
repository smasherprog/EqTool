using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EQTool.Services.Parsing.Helpers
{
    public class ParseHandleYouCasting
    {
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQSpells spells;
        private readonly LogEvents logEvents;
        private int UserCastingSpellCounter = 0;

        public ParseHandleYouCasting(ActivePlayer activePlayer, IAppDispatcher appDispatcher, EQSpells spells, LogEvents logEvents)
        {
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.spells = spells;
            this.logEvents = logEvents;
        }

        public void HandleYouBeginCastingSpellStart(string message, DateTime timestamp, int lineCounter)
        {
            var spellname = message.Substring(EQSpells.YouBeginCasting.Length - 1).Trim().TrimEnd('.');
            if (spells.YouCastSpells.TryGetValue(spellname, out var foundspells))
            {
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level);
                logEvents.Handle(new YouBeginCastingEvent { Line = message, LineCounter = lineCounter, TimeStamp = timestamp, Spell = foundspell });
                Debug.WriteLine($"Self Casting Spell: {spellname} Delay: {foundspell.casttime}");
                appDispatcher.DispatchUI(() =>
                {
                    if (activePlayer.Player != null)
                    {
                        if (foundspell.Classes.Count == 1)
                        {
                            if (!activePlayer.Player.PlayerClass.HasValue)
                            {
                                logEvents.Handle(new ClassDetectedEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = message, PlayerClass = foundspell.Classes.FirstOrDefault().Key });
                            }

                            if (activePlayer.Player.Level < foundspell.Classes.FirstOrDefault().Value)
                            {
                                logEvents.Handle(new PlayerLevelDetectionEvent { TimeStamp = timestamp, LineCounter = lineCounter, Line = message, PlayerLevel = foundspell.Classes.FirstOrDefault().Value });
                            }
                        }
                    }

                    activePlayer.UserCastingSpell = foundspell;
                    if (activePlayer.UserCastingSpell.casttime > 0)
                    {
                        UserCastingSpellCounter++;
                        _ = Task.Delay(activePlayer.UserCastingSpell.casttime * 2).ContinueWith(a =>
                        {
                            Debug.WriteLine($"Cleaning Spell");
                            appDispatcher.DispatchUI(() =>
                            {
                                if (--UserCastingSpellCounter <= 0)
                                {
                                    UserCastingSpellCounter = 0;
                                    activePlayer.UserCastingSpell = null;
                                }
                            });
                        });
                    }
                });
            }
        }

        public SpellCastEvent HandleYouSpell(string message, DateTime timestamp, int lineCounter)
        {
            if (spells.CastOnYouSpells.TryGetValue(message, out var foundspells))
            {
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level);
                Debug.WriteLine($"You Casting Spell: {message} Delay: {foundspell.casttime}");
                return new SpellCastEvent
                {
                    Spell = foundspell,
                    TargetName = EQSpells.SpaceYou,
                    TimeStamp = timestamp,
                    Line = message,
                    CastByYou = false,
                    LineCounter = lineCounter
                };
            }

            return null;
        }

        public SpellCastEvent HandleYourSpell(string message, DateTime timestamp, int lineCounter)
        {
            if (message == "Your Pegasus Feather Cloak begins to glow.")
            {
                appDispatcher.DispatchUI(() =>
                {
                    var peggyspell = spells.AllSpells.FirstOrDefault(a => a.name == "Peggy Levitate");
                    activePlayer.UserCastingSpell = peggyspell;
                    UserCastingSpellCounter++;
                    _ = Task.Delay(activePlayer.UserCastingSpell.casttime * 2).ContinueWith(a =>
                    {
                        Debug.WriteLine($"Cleaning Spell");
                        appDispatcher.DispatchUI(() =>
                        {
                            if (--UserCastingSpellCounter <= 0)
                            {
                                UserCastingSpellCounter = 0;
                                activePlayer.UserCastingSpell = null;
                            }
                        });
                    });
                });
                return null;
            }
            if (spells.CastOnYouSpells.TryGetValue(message, out var foundspells))
            {
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level);
                Debug.WriteLine($"Your Casting Spell: {message} Delay: {foundspell.casttime}");
                return new SpellCastEvent
                {
                    Spell = foundspell,
                    TargetName = EQSpells.SpaceYou,
                    TimeStamp = timestamp,
                    Line = message,
                    CastByYou = false,
                    LineCounter = lineCounter
                };
            }

            return null;
        }

        public SpellCastEvent HandleYouBeginCastingSpellEnd(string message, DateTime timestamp, int lineCounter)
        {
            Debug.WriteLine($"Self Finished Spell: {message}");
            var spell = activePlayer.UserCastingSpell;
            appDispatcher.DispatchUI(() =>
            {
                activePlayer.UserCastingSpell = null;
            });
            return new SpellCastEvent
            {
                Spell = spell,
                TargetName = EQSpells.SpaceYou,
                CastByYou = true,
                TimeStamp = timestamp,
                Line = message,
                LineCounter = lineCounter
            };
        }

        public SpellCastEvent HandleYouBeginCastingSpellOtherEnd(string message, DateTime timestamp, int lineCounter)
        {
            var targetname = message.Replace(activePlayer.UserCastingSpell.cast_on_other, string.Empty).Trim();
            Debug.WriteLine($"Self Finished Spell: {message}");
            var spell = activePlayer.UserCastingSpell;
            return new SpellCastEvent
            {
                Spell = spell,
                TargetName = targetname,
                CastByYou = true,
                TimeStamp = timestamp,
                Line = message,
                LineCounter = lineCounter
            };
        }
    }
}
