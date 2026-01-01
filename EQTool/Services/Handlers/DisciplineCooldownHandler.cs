using System;
using System.Windows.Media;
using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;

namespace EQTool.Services.Handlers
{
    public class DisciplineCooldownHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        
        public DisciplineCooldownHandler(EQSpells spells, SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.DisciplineCooldownEvent += LogEvents_DisciplineCooldownEvent;
            this.spells = spells;
        }
        
        private void LogEvents_DisciplineCooldownEvent(object sender, DisciplineCooldownEvent discCooldownEvent)
        {
            if (spells.AllSpells.TryGetValue("Feign Death", out var timerVisuals))
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    Id = discCooldownEvent.DisciplineName,
                    Target = CustomTimer.CustomerTime,
                    Rect = timerVisuals.Rect,
                    Icon = timerVisuals.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(discCooldownEvent.TotalTimerSeconds),
                    TotalRemainingDuration = TimeSpan.FromSeconds(discCooldownEvent.TotalTimerSeconds),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.DarkSeaGreen
                });
            }
        }
    }
}
