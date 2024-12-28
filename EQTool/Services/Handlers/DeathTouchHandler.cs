using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class DeathTouchHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        private readonly List<string> NPCsThatDeathTouch = new List<string>()
        {
            "Fright",
            "Dread"
        };

        public DeathTouchHandler(EQSpells spells,
            SpellWindowViewModel spellWindowViewModel,
            PlayerTrackerService playerTrackerService, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.CommsEvent += LogEvents_CommsEvent;
        }

        private void LogEvents_CommsEvent(object sender, CommsEvent e)
        {
            if (!NPCsThatDeathTouch.Contains(e.Sender) || e.Content.Contains(' '))
            {
                return;
            }

            var match = spells.AllSpells.FirstOrDefault(a => a.name == "Disease Cloud");
            spellWindowViewModel.TryAdd(new TimerViewModel
            {
                TotalDuration = TimeSpan.FromSeconds(45),
                Name = $"--DT-- '{e.Content}'",
                Icon = match.SpellIcon,
                Rect = match.Rect,
                PercentLeft = 100,
                GroupName = CustomTimer.CustomerTime,
                TotalRemainingDuration = TimeSpan.FromSeconds(45),
                UpdatedDateTime = DateTime.Now,
            });
        }
    }
}
