using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class SpellYouBeginToCastHandler : BaseHandler
    {
        private readonly List<string> SpellsThatNeedTimers = new List<string>()
        {
            "Dictate"
        };

        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        private readonly IAppDispatcher appDispatcher;

        public SpellYouBeginToCastHandler(
            EQSpells spells,
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            IAppDispatcher appDispatcher,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.appDispatcher = appDispatcher;
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.YouBeginCastingEvent += LogEvents_YouBeginCastingEvent; ;
        }

        private void LogEvents_YouBeginCastingEvent(object sender, YouBeginCastingEvent e)
        {
            var spellname = e.Spell.name;
            if (SpellsThatNeedTimers.Contains(e.Spell.name))
            {
                appDispatcher.DispatchUI(() =>
                {
                    var spell = spellWindowViewModel.SpellList.Where(a => a.GroupName == CustomTimer.CustomerTime && a.SpellViewModelType == SpellViewModelType.Timer).FirstOrDefault();
                    if (spell != null)
                    {
                        _ = spellWindowViewModel.SpellList.Remove(spell);
                    }
                });

                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = spellname,
                    Rect = e.Spell.Rect,
                    Icon = e.Spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds((int)(e.Spell.recastTime / 1000.0)),
                    TotalRemainingDuration = TimeSpan.FromSeconds((int)(e.Spell.recastTime / 1000.0)),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.SkyBlue
                });
            }
        }
    }
}
