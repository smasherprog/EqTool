using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;


namespace EQTool.Services.Handlers
{
    public class DisciplineCooldownHandler : BaseHandler
    {
        // internal data
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        // ctor
        public DisciplineCooldownHandler(EQSpells spells, SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.DisciplineCooldownEvent += LogEvents_DisciplineCooldownEvent;
            this.spells = spells;
        }

        //
        // function that gets called whenever a DisciplineCooldownEvent is received
        //
        private void LogEvents_DisciplineCooldownEvent(object sender, DisciplineCooldownEvent discCooldownEvent)
        {
            var spellname = "Feign Death";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            spellWindowViewModel.TryAdd(new TimerViewModel
            {
                PercentLeft = 100,
                Id = discCooldownEvent.DisciplineName,
                Target = CustomTimer.CustomerTime,
                Rect = spell.Rect,
                Icon = spell.SpellIcon,
                TotalDuration = TimeSpan.FromSeconds(discCooldownEvent.TotalTimerSeconds),
                TotalRemainingDuration = TimeSpan.FromSeconds(discCooldownEvent.TotalTimerSeconds),
                UpdatedDateTime = DateTime.Now,
                ProgressBarColor = Brushes.DarkSeaGreen
            });
        }
    }
}
