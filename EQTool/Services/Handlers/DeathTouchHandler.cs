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
        private readonly PlayerTrackerService playerTrackerService;
        private readonly EQSpells spells;
        private readonly List<string> NPCsThatDeathTouch = new List<string>()
        {
            "Fright",
            "Dread"
        };

        public DeathTouchHandler(
            SpellWindowViewModel spellWindowViewModel,
            PlayerTrackerService playerTrackerService,
            LogEvents logEvents,
            EQSpells spells,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spells = spells;
            this.playerTrackerService = playerTrackerService;
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.CommsEvent += LogEvents_CommsEvent;
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
