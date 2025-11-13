using System;
using System.Collections.Generic;
using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class YouForgetHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public YouForgetHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, BaseHandlerData baseHandlerData)
            : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
            
            logEvents.YouForgetEvent += LogEvents_YouForgetEvent;
        }

        private void LogEvents_YouForgetEvent(object sender, YouForgetEvent e)
        {
            if (!spells.AllSpells.TryGetValue(e.SpellName, out var spell))
                return;
                
            var durationSeconds = TimeSpan.FromSeconds((int)(spell.recastTime / 1000.0));
            if (durationSeconds < SpellHandlerService.MinimumRecastForYouCooldownTimer)
                return;
            
            spellWindowViewModel.TryRemoveUnambiguousSpellSelf(new List<string> { $"{e.SpellName} Cooldown" });
        }
    }
}
