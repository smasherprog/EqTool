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
        private readonly EQSpells spells;

        public RingWarHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.RingWarEvent += LogEvents_RingWarEvent;
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
        }

        private void LogEvents_RingWarEvent(object sender, RingWarEvent e)
        {
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == "Disease Cloud");
            var startingtime = 0;
            for (var round = 1; round <= 3; round++)
            {
                for (var i = 1; i <= 7; i++)
                {
                    startingtime += 210;
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        Target = $" Wave {round} Ring War",
                        Id = $"Round {i}",
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        TotalDuration = TimeSpan.FromSeconds(startingtime),
                        TotalRemainingDuration = TimeSpan.FromSeconds(startingtime),
                        UpdatedDateTime = DateTime.Now,
                        ProgressBarColor = Brushes.LightSeaGreen
                    });
                }
                startingtime += 300;
                if (round == 3)
                {
                    startingtime += 4;
                }

                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    Target = $" Wave {round} Ring War",
                    Id = $"-- Break --",
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(startingtime),
                    TotalRemainingDuration = TimeSpan.FromSeconds(startingtime),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.MediumAquamarine
                });
            }
        }
    }
}
