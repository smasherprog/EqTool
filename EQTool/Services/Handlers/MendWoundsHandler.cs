using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class MendWoundsHandler : BaseHandler
    {
        private readonly Spell HealSpell;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public MendWoundsHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.MendWoundsEvent += LogEvents_MendWoundsEvent;
            HealSpell = spells.AllSpells.FirstOrDefault(a => a.name == "Chloroplast") ?? spells.AllSpells.FirstOrDefault(a => a.name == "Regeneration");
        }

        private void LogEvents_MendWoundsEvent(object sender, MendWoundsEvent e)
        {
            var spellduration = TimeSpan.FromMinutes(6);
            var vm = new SpellViewModel
            {
                UpdatedDateTime = DateTime.Now,
                PercentLeft = 100,
                BenefitDetriment = HealSpell.benefit_detriment,
                SpellType = HealSpell.SpellType,
                Id = "Mend",
                Target = EQSpells.SpaceYou,
                Rect = HealSpell.Rect,
                Icon = HealSpell.SpellIcon,
                Classes = HealSpell.Classes,
                TotalDuration = spellduration,
                TotalRemainingDuration = spellduration,
                TargetClass = EQToolShared.Enums.PlayerClasses.Monk
            };
            spellWindowViewModel.TryAdd(vm);
        }
    }
}
