using System;
using System.Linq;
using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared.Enums;

namespace EQTool.Services.Handlers
{
    public class CooldownHandlerService
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly ActivePlayer activePlayer;
        private readonly CasterGuessingService casterGuessingService;
        private readonly PlayerTrackerService playerTrackerService;

        public CooldownHandlerService(ActivePlayer activePlayer, CasterGuessingService casterGuessingService, PlayerTrackerService playerTrackerService, SpellWindowViewModel spellWindowViewModel)
        {
            this.activePlayer = activePlayer;
            this.casterGuessingService = casterGuessingService;
            this.playerTrackerService = playerTrackerService;
            this.spellWindowViewModel = spellWindowViewModel;
        }

        public void AddCooldownTimerIfNecessary(Spell spell, string casterName, string targetName, bool targetIsNpc, int delayOffset, DateTime timeStamp)
        {
            if (spell.name.EndsWith("Recourse", StringComparison.OrdinalIgnoreCase))    // Recourse effects tend to mirror their counterpart and are already handled elsewhere
                return;
            
            var spellName = spell.name;
            var timerId = $"{spellName} Cooldown";
            
            targetName = CleanupTargetName(targetIsNpc, targetName);
            
            if (string.IsNullOrWhiteSpace(casterName))
                casterName = casterGuessingService.TryGuessNameForTimer(spell, timerId, targetName, requireCertainty: false);

            var cooldownRecipient = TargetOnlyIfUnknownCaster(casterName, targetName);
            var cooldownRecipientClass = playerTrackerService.GetPlayer(cooldownRecipient)?.PlayerClass;
            
            // TODO: Factor in the log timestamp, the delayOffset, and the "current time" to better project the "true" duration/remaining values. 
            
            var cooldown = TimeSpan.FromSeconds((int)(spell.recastTime / 1000.0)); 
            if (spell.name.EndsWith("Discipline", StringComparison.OrdinalIgnoreCase))
            {
                var discCooldown = TimeSpan.FromSeconds(GetDisciplineCooldownSeconds(spell, delayOffset));
                spellWindowViewModel.TryAdd(new SpellViewModel
                {
                    PercentLeft = 100,
                    Id = timerId,
                    Target = cooldownRecipient,
                    TargetClass = cooldownRecipientClass,
                    Caster = casterName,
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    Classes = spell.Classes,
                    BenefitDetriment = SpellBenefitDetriment.Cooldown,
                    TotalDuration = discCooldown,
                    TotalRemainingDuration = discCooldown,
                    UpdatedDateTime = DateTime.Now
                });
            }
            else if ((cooldownRecipient == EQSpells.SpaceYou && cooldown >= EQSpells.MinimumRecastForYouCooldownTimer)
                 || (cooldownRecipient != EQSpells.SpaceYou && cooldown >= EQSpells.MinimumRecastForOtherCooldownTimer))
            {
                spellWindowViewModel.TryAdd(new SpellViewModel
                {
                    PercentLeft = 100,
                    Id = timerId,
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
        }

        //TODO: Re-write this a little so that we can separate the "True Cooldown" and the "Time Remaining" as two separate values
        private int GetDisciplineCooldownSeconds(Spell spell, int delayOffset)
        {
            var baseTime = (int)((spell.recastTime + delayOffset) / 1000.0);
            var playerLevel = activePlayer.Player.Level;
            if (spell.name == "Evasive Discipline")
            {
                float baseSeconds = 15 * 60;
                float levelRange = 60 - 52;
                float secondsRange = (15 - 7) * 60;
                var secondsPerLevelRange = secondsRange / levelRange;
                float playerLevelTick = Math.Max(playerLevel, 52) - 52;
                baseTime = (int)(baseSeconds - (playerLevelTick * secondsPerLevelRange));
            }
            else if (spell.name == "Defensive Discipline")
            {
                float baseSeconds = 15 * 60;
                float levelRange = 60 - 55;
                float secondsRange = (15 - 10) * 60;
                var secondsPerLevelRange = secondsRange / levelRange;
                float playerLevelTick = Math.Max(playerLevel, 55) - 55;
                baseTime = (int)(baseSeconds - (playerLevelTick * secondsPerLevelRange));
            }
            else if (spell.name == "Precision Discipline")
            {
                float baseSeconds = 30 * 60;
                float levelRange = 60 - 57;
                float secondsRange = (30 - 27) * 60;
                var secondsPerLevelRange = secondsRange / levelRange;
                float playerLevelTick = Math.Max(playerLevel, 57) - 57;
                baseTime = (int)(baseSeconds - (playerLevelTick * secondsPerLevelRange));
            }
            else if (spell.name == "Stonestance Discipline")
            {
                float baseSeconds = 12 * 60;
                float levelRange = 60 - 51;
                float secondsRange = (12 - 4) * 60;
                var secondsPerLevelRange = secondsRange / levelRange;
                float playerLevelTick = Math.Max(playerLevel, 51) - 51;
                baseTime = (int)(baseSeconds - (playerLevelTick * secondsPerLevelRange));
            }
            else if (spell.name == "Voiddance Discipline")
            {
                float baseSeconds = 60 * 60;
                float levelRange = 60 - 54;
                float secondsRange = (60 - 54) * 60;
                var secondsPerLevelRange = secondsRange / levelRange;
                float playerLevelTick = Math.Max(playerLevel, 54) - 54;
                baseTime = (int)(baseSeconds - (playerLevelTick * secondsPerLevelRange));
            }
            else if (spell.name == "Innerflame Discipline")
            {
                float baseSeconds = 60 * 60;
                float levelRange = 60 - 56;
                float secondsRange = (30 - 26) * 60;
                var secondsPerLevelRange = secondsRange / levelRange;
                float playerLevelTick = Math.Max(playerLevel, 56) - 56;
                baseTime = (int)(baseSeconds - (playerLevelTick * secondsPerLevelRange));
            }

            return baseTime;
        }

        private static string CleanupTargetName(bool targetIsNpc, string targetName)
        {
            if (targetName != null && targetIsNpc && !targetName.StartsWith(" "))
                targetName = $" {targetName}";
            
            return targetName;
        }
        
        private static string TargetOnlyIfUnknownCaster(string casterName, string target) => string.IsNullOrWhiteSpace(casterName) ? target : casterName;
    }
}
