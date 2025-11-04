using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class AOWTimerHandler : BaseHandler
    {
        private readonly EQSpells spells;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public AOWTimerHandler(EQSpells spells, SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.LineEvent += LogEvents_LineEvent;
        }

        private void LogEvents_LineEvent(object sender, LineEvent e)
        {
            if (e.Line != "The Avatar of War shouts 'Who dares defile my temple?! Come forth and face me!'")
            {
                return;
            }

            spells.AllSpells.TryGetValue("Spirit of Wolf", out var spell);
            if (spell == null)
            {
                return;
            }
                
            appDispatcher.DispatchUI(() =>
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    Target = CustomTimer.CustomerTime,
                    Id = $"The Avatar of War Lockout",
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromMinutes(20),
                    TotalRemainingDuration = TimeSpan.FromMinutes(20),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.Orchid
                }, true);
            });
        }
    }
}
