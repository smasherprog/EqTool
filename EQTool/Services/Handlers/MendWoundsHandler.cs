using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class MendWoundsHandler : BaseHandler
    {
        private readonly IAppDispatcher appDispatcher;
        private readonly Spell HealSpell;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public MendWoundsHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, IAppDispatcher appDispatcher, LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.appDispatcher = appDispatcher;
            this.logEvents.MendWoundsEvent += LogEvents_MendWoundsEvent;
            HealSpell = spells.AllSpells.FirstOrDefault(a => a.name == "Chloroplast") ?? spells.AllSpells.FirstOrDefault(a => a.name == "Regeneration");
        }

        private void LogEvents_MendWoundsEvent(object sender, MendWoundsEvent e)
        {
            var spellduration = TimeSpan.FromMinutes(6); 
            var vm = new SpellViewModel
            {
                UpdatedDateTime = DateTime.Now,
                PercentLeft = 100,
                Type = HealSpell.type,
                SpellType = HealSpell.SpellType,
                GroupName = EQSpells.SpaceYou,
                Name = "Mend",
                Rect = HealSpell.Rect,
                Icon = HealSpell.SpellIcon,
                Classes = HealSpell.Classes,
                TotalDuration = spellduration,
                TotalRemainingDuration = spellduration
            };
            spellWindowViewModel.TryAdd(vm);
        } 
    }
}
