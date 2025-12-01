using System;
using System.Collections.Generic;
using System.Linq;
using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared;
using EQToolShared.Enums;

namespace EQTool.Services
{
    public class CasterGuessingService
    {
        private readonly ActivePlayer activePlayer;
        private readonly PlayerTrackerService playerTrackerService;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public CasterGuessingService(ActivePlayer activePlayer, PlayerTrackerService playerTrackerService, SpellWindowViewModel spellWindowViewModel)
        {
            this.activePlayer = activePlayer;
            this.playerTrackerService = playerTrackerService;
            this.spellWindowViewModel = spellWindowViewModel;
        }
        
        public string TryGuessNameForTimer(Spell spell, string targetName, bool requireCertainty)
            => TryGuessNameForTimer(spell, spell?.name, targetName, requireCertainty);
        
        public string TryGuessNameForTimer(Spell spell, string timerId, string targetName, bool requireCertainty)
        {
            if (spell == null)
                return string.Empty;

            timerId = timerId ?? string.Empty;
            if (spell.SpellType == SpellType.Self || timerId.Contains("Discipline", StringComparison.OrdinalIgnoreCase))
                return targetName;

            if (!spell.Classes.Any() && string.IsNullOrWhiteSpace(timerId))
                return string.Empty;    // We have no information. It won't be possible to figure it out.

            // Instant casts could be you, could be npcs, could be anybody. If we require certainty, it's physically impossible to know for sure.
            var isInstantCast = spell.casttime == 0;
            if (isInstantCast && requireCertainty)
                return string.Empty;
            
            var isNpcTarget = MasterNPCList.NPCs.Contains(targetName.Trim());
            switch (spell.benefit_detriment)
            {
                case SpellBenefitDetriment.Detrimental when !isNpcTarget:   // Could be a mob debuffing a player, but it isn't that helpful to know that. (yet)
                case SpellBenefitDetriment.Beneficial when isNpcTarget:     // Could be a charm pet that someone is buffing, but it isn't worth guessing since mobs self buff all the time.
                    return string.Empty;
            }
            
            //TODO: Pull from a list of "active casters" in addition to (or instead of) this.
            var potentialCasters = playerTrackerService.GetNearbyClasses(spell.Classes.Keys).ToList();
            if (timerId.EndsWith("Cooldown") && potentialCasters.Any())
            {
                var existingCdRecipients = spellWindowViewModel.SpellList
                    .Where(x => x.Id.Equals(timerId))
                    .Select(x => x.Target).ToList();
                
                // Purge our potential list of existing CD recipients since it obviously cannot be them if their ability is still on cooldown.
                if (existingCdRecipients.Any())
                {
                    var impossibleCasters = potentialCasters.Where(caster => existingCdRecipients.Contains(caster.Name));
                    foreach (var impossibleCaster in impossibleCasters)
                        potentialCasters.Remove(impossibleCaster);
                }
            }

            if (!potentialCasters.Any())
                return string.Empty;

            var playerName = activePlayer.Player?.Name;
            
            string casterName = null;
            if (potentialCasters.Count == 1)
            {
                var name = potentialCasters.First().Name;
                
                // If the only available caster is the player, the only way we couldn't know this by now is if it's an instant cast.
                if (name != playerName || (name == playerName && isInstantCast))
                    casterName = name;
            }
            else if (!requireCertainty && potentialCasters.Count > 1)
            {
                // if (spell.Classes.Count == 1)
                // {
                    if (isInstantCast && activePlayer.Player?.PlayerClass == spell.Classes.First().Key)
                        casterName = EQSpells.SpaceYou; // For instant casts, better to err on the side of giving it to the player.
                    else
                        casterName = potentialCasters.First(x => x.Name != playerName).Name;
                // }
                // else
                // {
                //     // TODO: Figure out a good heuristic here.
                // }
            }
            
            if (casterName != null && casterName == playerName)
                casterName = EQSpells.SpaceYou;
            
            return casterName;
        }
    }
}
