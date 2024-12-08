using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class RingWarHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly IAppDispatcher appDispatcher;
        private readonly EQSpells spells;

        public RingWarHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach, IAppDispatcher appDispatcher, SpellWindowViewModel spellWindowViewModel, EQSpells spells) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.RingWarEvent += LogEvents_RingWarEvent;
            this.appDispatcher = appDispatcher;
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
        }

        private void LogEvents_RingWarEvent(object sender, RingWarEvent e)
        {
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == "Disease Cloud");
            var startingtime = 0;
            for (var i = 1; i <= 7; i++)
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = $" Wave {e.RoundNumber} Ring War",
                    Name = $"Round {i}",
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(startingtime + 210),
                    TotalRemainingDuration = TimeSpan.FromSeconds(startingtime + 210),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.LightSeaGreen
                });
                startingtime += 210;
            }
            spellWindowViewModel.TryAdd(new TimerViewModel
            {
                PercentLeft = 100,
                GroupName = $" Wave {e.RoundNumber} Ring War",
                Name = $"-- Break --",
                Rect = spell.Rect,
                Icon = spell.SpellIcon,
                TotalDuration = TimeSpan.FromSeconds(startingtime + 300),
                TotalRemainingDuration = TimeSpan.FromSeconds(startingtime + 300),
                UpdatedDateTime = DateTime.Now,
                ProgressBarColor = Brushes.MediumAquamarine
            });
        }
    }
}
