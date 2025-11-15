using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using EQToolShared.Enums;

namespace EQTool.Services.Handlers
{
    public class MendWoundsHandler : BaseHandler
    {
        private readonly Spell TimerVisuals;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public MendWoundsHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.MendWoundsEvent += LogEvents_MendWoundsEvent;
            
            if (!spells.AllSpells.TryGetValue("Chloroplast", out var spell))
            {
                spells.AllSpells.TryGetValue("Regeneration", out spell);
            }
            
            TimerVisuals = spell;
        }

        private void LogEvents_MendWoundsEvent(object sender, MendWoundsEvent e)
        {
            var spellduration = TimeSpan.FromMinutes(6);
            var vm = new SpellViewModel
            {
                UpdatedDateTime = DateTime.Now,
                PercentLeft = 100,
                BenefitDetriment = SpellBenefitDetriment.Cooldown,
                SpellType = SpellType.Self,
                Id = "Mend Cooldown",
                Target = EQSpells.SpaceYou,
                Rect = TimerVisuals.Rect,
                Icon = TimerVisuals.SpellIcon,
                Classes = TimerVisuals.Classes,
                TotalDuration = spellduration,
                TotalRemainingDuration = spellduration,
                TargetClass = EQToolShared.Enums.PlayerClasses.Monk
            };
            spellWindowViewModel.TryAdd(vm);
        }
    }
}
