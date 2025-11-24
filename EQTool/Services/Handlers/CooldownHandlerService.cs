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
        private readonly PlayerTrackerService playerTrackerService;

        public CooldownHandlerService(PlayerTrackerService playerTrackerService, SpellWindowViewModel spellWindowViewModel, ActivePlayer activePlayer)
        {
            this.playerTrackerService = playerTrackerService;
            this.spellWindowViewModel = spellWindowViewModel;
            this.activePlayer = activePlayer;
        }

        public void AddCooldownTimerIfNecessary(Spell spell, string casterName, string targetName, bool targetIsNpc, int delayOffset, DateTime timeStamp)
        {
            if (spell.name.EndsWith("Recourse", StringComparison.OrdinalIgnoreCase))    // Recourse effects tend to mirror their counterpart and are already handled elsewhere
                return;
            
            var spellName = spell.name;
            
            casterName = CleanupCasterName(spell, casterName, targetName);
            targetName = CleanupTargetName(targetIsNpc, targetName);
            casterName = TryGuessCasterNameForLayHandsIfNecessary(spell, casterName);

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
                    Id = $"{spellName} Cooldown",
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
        }

        private static string CleanupTargetName(bool targetIsNpc, string targetName)
        {
            if (targetName != null && targetIsNpc && !targetName.StartsWith(" "))
                targetName = $" {targetName}";
            
            return targetName;
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
        
        // TODO: This guesswork engine could be part of a greater whole in the future. For now it's just a lazy proof of concept specifically for layhands.
        private string TryGuessCasterNameForLayHandsIfNecessary(Spell spell, string casterName)
        {
            if (spell.name != "Lay on Hands" || !string.IsNullOrWhiteSpace(casterName))
                return casterName;

            var potentialCasters = playerTrackerService.GetNearbyClasses(PlayerClasses.Paladin).Select(x => x.Name).ToList();
            var existingCds = spellWindowViewModel.SpellList.Where(x => x.Id.Equals("Lay on Hands Cooldown"));
            if (existingCds.Any())
            {
                foreach (var cd in existingCds.Where(cd => potentialCasters.Contains(cd.Target)))
                    potentialCasters.Remove(cd.Target);
            }

            if (potentialCasters.Count == 1)
            {
                casterName = potentialCasters.First();
                if (casterName == activePlayer?.Player?.Name)
                    casterName = EQSpells.SpaceYou;
            }
            else if (potentialCasters.Count > 1)
            {
                if (activePlayer?.Player?.PlayerClass == PlayerClasses.Paladin)
                    casterName = EQSpells.SpaceYou; // Just let the player have it. They can figure it out themselves.
                else
                    casterName = potentialCasters.First();
            }
            
            return casterName;
        }

        private static string TargetOnlyIfUnknownCaster(string casterName, string target) => string.IsNullOrWhiteSpace(casterName) ? target : casterName;
    }
}
