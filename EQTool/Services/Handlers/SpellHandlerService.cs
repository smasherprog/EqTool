using System;
using System.Linq;
using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.Enums;

namespace EQTool.Services.Handlers
{
    public class SpellHandlerService
    {
        private readonly CooldownHandlerService cooldownService;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly ActivePlayer activePlayer;
        private readonly PlayerTrackerService playerTrackerService;

        public SpellHandlerService(CooldownHandlerService cooldownService, PlayerTrackerService playerTrackerService, SpellWindowViewModel spellWindowViewModel, ActivePlayer activePlayer)
        {
            this.cooldownService = cooldownService;
            this.playerTrackerService = playerTrackerService;
            this.spellWindowViewModel = spellWindowViewModel;
            this.activePlayer = activePlayer;
        }

        public void Handle(Spell spell, string casterName, string targetName, int delayOffset, DateTime timestamp)
        {
            var spellName = spell.name;
            var targetClass = playerTrackerService.GetPlayer(targetName)?.PlayerClass;
            var target = targetName;
            var isNpcTarget = false;
            if (MasterNPCList.NPCs.Contains(target.Trim()))
            {
                target = " " + target.Trim();
                isNpcTarget = true;
            }
            casterName = CleanupCasterName(spell, casterName, targetName);
            
            cooldownService.AddCooldownTimerIfNecessary(spell, casterName, targetName, isNpcTarget, delayOffset, timestamp);
            
            var needsCount = EQSpells.SpellsThatNeedCounts.Contains(spellName);
            if (needsCount)
            {
                var vm = new CounterViewModel
                {
                    UpdatedDateTime = DateTime.Now,
                    BenefitDetriment = spell.benefit_detriment,
                    Target = target,
                    TargetClass = targetClass,
                    Caster = casterName,
                    Id = $"{spellName}",
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
                    if (isNpcTarget && spell.benefit_detriment == SpellBenefitDetriment.Detrimental)
                    {
                        //TODO: This could be better. Maybe just do a time diff on the log timestamp vs datetime now?
                        var serverTick = TimeSpan.FromMilliseconds(6000);
                        spellDuration = spellDuration.Add(serverTick);
                        remainingDuration = remainingDuration.Add(serverTick);
                    }
                    if (activePlayer?.Player?.TimerRecastSetting == TimerRecast.StartNewTimer
                        && (spell.benefit_detriment == SpellBenefitDetriment.Detrimental
                        || EQSpells.Lulls.Any(x => x.Equals(spellName, StringComparison.OrdinalIgnoreCase))))
                    {
                        overWrite = false;
                    }
                    
                    var vm = new SpellViewModel
                    {
                        UpdatedDateTime = DateTime.Now,
                        PercentLeft = 100,
                        BenefitDetriment = spell.benefit_detriment,
                        Id = spellName,
                        Target = target,
                        Caster = casterName,
                        SpellType = spell.SpellType,
                        TargetClass = targetClass,
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
        
        private static string CleanupCasterName(Spell spell, string casterName, string targetName)
        {
            // Try and fix up the caster if it can be easily inferred and is empty.
            if (string.IsNullOrWhiteSpace(casterName))
            {
                if (spell.name.Contains("Discipline", StringComparison.OrdinalIgnoreCase)
                    || spell.SpellType == SpellType.Self)   // Shouldn't be necessary tbh but doesn't hurt to have a failsafe here
                {
                    casterName = targetName;
                }
            }

            return casterName;
        }
    }
}
