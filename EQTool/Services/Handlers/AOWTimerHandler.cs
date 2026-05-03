using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
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
            if (e.Line == "The Avatar of War shouts 'Who dares defile my temple?! Come forth and face me!'")
            {
                _ = spells.AllSpells.TryGetValue("Spirit of Wolf", out var spell);
                appDispatcher.DispatchUI(() =>
                {
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        GroupName = CustomTimer.CustomerTime,
                        Name = $"The Avatar of War Lockout",
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
}
