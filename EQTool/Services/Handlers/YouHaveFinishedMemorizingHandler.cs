using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class YouHaveFinishedMemorizingHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public YouHaveFinishedMemorizingHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;

            // catch FinishedMemorizing events
            logEvents.YouHaveFinishedMemorizingEvent += LogEvents_YouHaveFinishedMemorizingEvent;
        }

        private void LogEvents_YouHaveFinishedMemorizingEvent(object sender, YouHaveFinishedMemorizingEvent e)
        {
            if (SpellHandlerService.SpellsThatNeedTimers.Contains(e.SpellName))
            {
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == e.SpellName);
                var timerName = $"{e.SpellName} Cooldown";
                var durationSeconds = spell.recastTime / 1000;

                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = EQSpells.SpaceYou,
                    Name = timerName,
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(durationSeconds),
                    TotalRemainingDuration = TimeSpan.FromSeconds(durationSeconds),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.SkyBlue
                });
            }
        }
    }
}
