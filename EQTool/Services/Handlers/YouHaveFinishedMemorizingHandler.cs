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
        private readonly IAppDispatcher appDispatcher;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public YouHaveFinishedMemorizingHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, IAppDispatcher appDispatcher, LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
            this.appDispatcher = appDispatcher;
            this.logEvents.YouHaveFinishedMemorizingEvent += LogEvents_YouHaveFinishedMemorizingEvent;
        }

        private void LogEvents_YouHaveFinishedMemorizingEvent(object sender, YouHaveFinishedMemorizingEvent e)
        {
            if (BaseSpellYouCastingHandler.SpellsThatNeedTimers.Contains(e.SpellName))
            {
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == e.SpellName);
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = EQSpells.SpaceYou,
                    Name = e.SpellName,
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds((int)(spell.recastTime / 1000.0)),
                    TotalRemainingDuration = TimeSpan.FromSeconds((int)(spell.recastTime / 1000.0)),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.SkyBlue
                });
            }
        }
    }
}
