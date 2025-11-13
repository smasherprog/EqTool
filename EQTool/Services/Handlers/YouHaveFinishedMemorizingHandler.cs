using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Windows.Media;
using EQToolShared.Enums;

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
            if (!spells.AllSpells.TryGetValue(e.SpellName, out var spell))
            {
                return;
            }
                
            var durationSeconds = TimeSpan.FromSeconds((int)(spell.recastTime / 1000.0));
            if (durationSeconds < SpellHandlerService.MinimumRecastForYouCooldownTimer)
            {
                return;
            }
            
            var timerName = $"{e.SpellName} Cooldown";
            spellWindowViewModel.TryAdd(new SpellViewModel
            {
                PercentLeft = 100,
                Id = timerName,
                Target = EQSpells.SpaceYou,
                Caster = EQSpells.SpaceYou,
                Rect = spell.Rect,
                Icon = spell.SpellIcon,
                Classes = spell.Classes,
                BenefitDetriment = SpellBenefitDetriment.Cooldown,
                TotalDuration = durationSeconds,
                TotalRemainingDuration = durationSeconds,
                UpdatedDateTime = DateTime.Now
            });
        }
    }
}
