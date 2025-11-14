using System;
using System.Collections.Generic;
using System.Linq;
using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.Enums;
using EQToolShared.Extensions;

namespace EQTool.Services.Handlers
{
    public class SpellHandlerService
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly ActivePlayer activePlayer;
        private readonly PlayerTrackerService playerTrackerService;

        public SpellHandlerService(PlayerTrackerService playerTrackerService, SpellWindowViewModel spellWindowViewModel, ActivePlayer activePlayer)
        {
            this.playerTrackerService = playerTrackerService;
            this.spellWindowViewModel = spellWindowViewModel;
            this.activePlayer = activePlayer;
        }

        public void Handle(Spell spell, string casterName, string targetName, int delayOffset, DateTime timestamp)
        {
            var spellname = spell.name;
            var targetclass = playerTrackerService.GetPlayer(targetName)?.PlayerClass;
            var target = targetName;
            var isnpc = false;
            if (MasterNPCList.NPCs.Contains(target.Trim()))
            {
                target = " " + target.Trim();
                isnpc = true;
            }
            
            // Try and fix up the caster if it can be easily inferred and is empty.
            if (string.IsNullOrWhiteSpace(casterName))
            {
                if (spellname.Contains("Discipline", StringComparison.OrdinalIgnoreCase)
                || spell.SpellType == SpellType.Self)   // Shouldn't be necessary tbh but doesn't hurt to have a failsafe here.
                {
                    casterName = target;
                }
            }
            
            AddCooldownTimerIfNecessary(spell, casterName, target, delayOffset);
            
            var needscount = EQSpells.SpellsThatNeedCounts.Contains(spellname);
            if (needscount)
            {
                var vm = new CounterViewModel
                {
                    UpdatedDateTime = DateTime.Now,
                    BenefitDetriment = spell.benefit_detriment,
                    Target = target,
                    TargetClass = targetclass,
                    Caster = casterName,
                    Id = $"{spellname}",
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                };
                spellWindowViewModel.TryAdd(vm);
            }
            else
            {
                var spellDuration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(spell, activePlayer.Player?.PlayerClass, activePlayer.Player?.Level));
                if (spell.name == "Voiddance Discipline")
                {
                    spellDuration = TimeSpan.FromSeconds(8);
                }
                else if (spell.name == "Weapon Shield Discipline")
                {
                    spellDuration = TimeSpan.FromSeconds(20);
                }
                else if (spell.name == "Deftdance Discipline")
                {
                    spellDuration = TimeSpan.FromSeconds(15);
                }
                
                var remainingDuration = spellDuration.Add(TimeSpan.FromMilliseconds(delayOffset));
                if (spellDuration.TotalSeconds > 0)
                {
                    // figure out whether to overwrite/reset this timer
                    // the only time we don't want to overWrite is 
                    //  - target is NPC, and
                    //  - detrimental spell, and
                    //  - TimerRecast in StartNewTimer mode
                    var overWrite = true;
                    if (isnpc && spell.benefit_detriment == SpellBenefitDetriment.Detrimental)
                    {
                        //TODO: This could be better. Maybe just do a time diff on the log timestamp vs datetime now?
                        var serverTick = TimeSpan.FromMilliseconds(6000);
                        spellDuration = spellDuration.Add(serverTick);
                        remainingDuration = remainingDuration.Add(serverTick);
                    }
                    if (activePlayer?.Player?.TimerRecastSetting == TimerRecast.StartNewTimer
                        && (spell.benefit_detriment == SpellBenefitDetriment.Detrimental
                        || EQSpells.Lulls.Any(x => x.Equals(spellname, StringComparison.OrdinalIgnoreCase))))
                    {
                        overWrite = false;
                    }
                    
                    var vm = new SpellViewModel
                    {
                        UpdatedDateTime = DateTime.Now,
                        PercentLeft = 100,
                        BenefitDetriment = spell.benefit_detriment,
                        Id = spellname,
                        Target = target,
                        Caster = casterName,
                        SpellType = spell.SpellType,
                        TargetClass = targetclass,
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        Classes = spell.Classes,
                        TotalDuration = spellDuration,
                        TotalRemainingDuration = remainingDuration
                    };

                    // add the timer
                    spellWindowViewModel.TryAdd(vm, overWrite);
                }
            }
        }

        private void AddCooldownTimerIfNecessary(Spell spell, string casterName, string target, int delayOffset)
        {
            var spellName = spell.name;
            var cooldownRecipient = TargetOnlyIfUnknownCaster(casterName, target);
            var cooldownRecipientClass = playerTrackerService.GetPlayer(cooldownRecipient)?.PlayerClass;
            var cooldown = TimeSpan.FromSeconds((int)(spell.recastTime / 1000.0)); 
            
            if ((cooldownRecipient == EQSpells.SpaceYou && cooldown >= EQSpells.MinimumRecastForYouCooldownTimer)
            || (cooldownRecipient != EQSpells.SpaceYou && cooldown >= EQSpells.MinimumRecastForOtherCooldownTimer))
            {
                spellWindowViewModel.TryAdd(new SpellViewModel
                {
                    PercentLeft = 100,
                    Id = $"{spellName} Cooldown",
                    Target = cooldownRecipient,
                    TargetClass = cooldownRecipientClass,
                    Caster = casterName,
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    Classes = spell.Classes,
                    BenefitDetriment = SpellBenefitDetriment.Cooldown,
                    TotalDuration = cooldown,
                    TotalRemainingDuration = TimeSpan.FromSeconds((int)((spell.recastTime + delayOffset) / 1000.0)),
                    UpdatedDateTime = DateTime.Now
                });
            }
            else if (spell.name.EndsWith("Discipline"))
            {
                var basetime = (int)((spell.recastTime + delayOffset) / 1000.0);
                var playerlevel = activePlayer.Player.Level;
                if (spell.name == "Evasive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 52;
                    float secondsrange = (15 - 7) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = Math.Max(playerlevel, 52) - 52;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Defensive Discipline")
                {
                    float baseseconds = 15 * 60;
                    float levelrange = 60 - 55;
                    float secondsrange = (15 - 10) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = Math.Max(playerlevel, 55) - 55;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Precision Discipline")
                {
                    float baseseconds = 30 * 60;
                    float levelrange = 60 - 57;
                    float secondsrange = (30 - 27) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = Math.Max(playerlevel, 57) - 57;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Stonestance Discipline")
                {
                    float baseseconds = 12 * 60;
                    float levelrange = 60 - 51;
                    float secondsrange = (12 - 4) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = Math.Max(playerlevel, 51) - 51;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Voiddance Discipline")
                {
                    float baseseconds = 60 * 60;
                    float levelrange = 60 - 54;
                    float secondsrange = (60 - 54) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = Math.Max(playerlevel, 54) - 54;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }
                else if (spell.name == "Innerflame Discipline")
                {
                    float baseseconds = 60 * 60;
                    float levelrange = 60 - 56;
                    float secondsrange = (30 - 26) * 60;
                    var secondsperlevelrange = secondsrange / levelrange;
                    float playerleveltick = Math.Max(playerlevel, 56) - 56;
                    basetime = (int)(baseseconds - (playerleveltick * secondsperlevelrange));
                }

                spellWindowViewModel.TryAdd(new SpellViewModel
                {
                    PercentLeft = 100,
                    Id = $"{spellName} Cooldown",
                    Target = cooldownRecipient,
                    TargetClass = cooldownRecipientClass,
                    Caster = casterName,
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    Classes = spell.Classes,
                    BenefitDetriment = SpellBenefitDetriment.Cooldown,
                    TotalDuration = TimeSpan.FromSeconds(basetime),
                    TotalRemainingDuration = TimeSpan.FromSeconds(basetime),
                    UpdatedDateTime = DateTime.Now
                });
            }
        }

        private static string TargetOnlyIfUnknownCaster(string casterName, string target) => string.IsNullOrWhiteSpace(casterName) ? target : casterName;
    }
}
