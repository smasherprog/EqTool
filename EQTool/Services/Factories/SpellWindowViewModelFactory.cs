using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using EQToolShared.HubModels;
using System;
using System.Linq;

namespace EQTool.Services.Factories
{
    public class SpellWindowViewModelFactory
    {
        private readonly EQSpells spells;
        private readonly ActivePlayer activePlayer;

        public SpellWindowViewModelFactory(EQSpells spells, ActivePlayer activePlayer)
        {
            this.spells = spells;
            this.activePlayer = activePlayer;
        }

        public TimerViewModel Create(HubCustomTimer e)
        {
            var spellicon = spells.AllSpells.FirstOrDefault(a => a.name == e.SpellNameIcon);
            return new TimerViewModel
            {
                PercentLeft = 100,
                GroupName = CustomTimer.CustomerTime,
                Name = e.Name,
                Rect = spellicon.Rect,
                Icon = spellicon.SpellIcon,
                TotalDuration = TimeSpan.FromSeconds(e.DurationInSeconds),
                TotalRemainingDuration = TimeSpan.FromSeconds(e.DurationInSeconds),
                UpdatedDateTime = DateTime.Now
            };
        }

        public TimerViewModel Create(SlainEvent e)
        {
            var spellicon = spells.AllSpells.FirstOrDefault(a => a.name == "Disease Cloud");
            var zonetimer = ZoneSpawnTimes.GetSpawnTime(e.Victim, activePlayer?.Player?.Zone);
            var add = new TimerViewModel
            {
                PercentLeft = 100,
                GroupName = CustomTimer.CustomerTime,
                Name = "--Dead-- " + e.Victim,
                Rect = spellicon.Rect,
                Icon = spellicon.SpellIcon,
                TotalDuration = TimeSpan.FromSeconds((int)zonetimer.TotalSeconds),
                TotalRemainingDuration = TimeSpan.FromSeconds((int)zonetimer.TotalSeconds),
                UpdatedDateTime = DateTime.Now
            };
            return add;
        }

        public TimerViewModel Create(StartTimerEvent e)
        {
            var spellicon = spells.AllSpells.FirstOrDefault(a => a.name == e.CustomTimer.SpellNameIcon);
            return new TimerViewModel
            {
                PercentLeft = 100,
                GroupName = e.CustomTimer.TargetName,
                Name = e.CustomTimer.Name,
                Rect = spellicon.Rect,
                Icon = spellicon.SpellIcon,
                TotalDuration = TimeSpan.FromSeconds(e.CustomTimer.DurationSeconds),
                TotalRemainingDuration = TimeSpan.FromSeconds(e.CustomTimer.DurationSeconds),
                UpdatedDateTime = DateTime.Now
            };
        }
    }
}
