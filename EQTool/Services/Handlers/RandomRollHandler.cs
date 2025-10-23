using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class RandomRollHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public RandomRollHandler(
            SpellWindowViewModel spellWindowViewModel,
            EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.RandomRollEvent += LogEvents_RandomRollEvent;
        }

        private void LogEvents_RandomRollEvent(object sender, RandomRollEvent e)
        {
            var icon = spells.AllSpells.FirstOrDefault(a => a.name == "Invisibility");
            var trigger = new RollViewModel
            {
                MaxRoll = e.MaxRoll,
                TotalDuration = TimeSpan.FromMinutes(3),
                Icon = icon.SpellIcon,
                Name = e.PlayerName,
                Roll = e.Roll,
                PercentLeft = 100,
                Rect = icon.Rect,
                UpdatedDateTime = DateTime.Now,
                TotalRemainingDuration = TimeSpan.FromMinutes(3)
            };
            appDispatcher.DispatchUI(() =>
            {
                var rollsingroup = spellWindowViewModel.SpellList
                    .Where(a => a.Name == trigger.Name && a.GroupName == trigger.GroupName && a.SpellViewModelType == SpellViewModelType.Roll)
                    .Cast<RollViewModel>()
                    .ToList();
                var rollorder = rollsingroup.Select(a => (int?)a.RollOrder).Max() ?? 0;
                trigger.RollOrder = rollorder + 1;
                spellWindowViewModel.TryAdd(trigger);
            });
        }
    }
}
