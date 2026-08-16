using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class SpawnTimerHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public SpawnTimerHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
            logEvents.ExpGainedEvent += LogEvents_ExpGainedEvent;
            logEvents.SlainEvent += LogEvents_SlainEvent;
            logEvents.FactionEvent += LogEvents_FactionEvent;
        }

        public SpawnTimerDialogViewModel Model { get; } = new SpawnTimerDialogViewModel();

        private void LogEvents_ExpGainedEvent(object sender, ExpGainedEvent expGainedEvent)
        {
            Debug.WriteLine($"ExpGainedEvent: [{expGainedEvent.TimeStamp}] [{expGainedEvent.Line}]");

            if (Model.SpawnTimerEnabled && (Model.StartType == SpawnTimerDialogViewModel.StartTypes.EXP_MESSAGE))
            {
                // todo - carry Model's warning/end text and TTS settings into the timer objects

                // a spawn timer has no icon of its own, so it borrows this spell's artwork
                var spellname = "Feign Death";
                spells.AllSpells.TryGetValue(spellname, out var spell);
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = $"Exp Timer [{Model.GetNextTimerCounter()}]",
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                    TotalRemainingDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.DarkSeaGreen
                });
            }
        }

        private void LogEvents_SlainEvent(object sender, SlainEvent slainEvent)
        {
            Debug.WriteLine($"SlainEvent: [{slainEvent.TimeStamp}], Killer = [{slainEvent.Killer}], Victim = [{slainEvent.Victim}]");

            if (Model.SpawnTimerEnabled && (Model.StartType == SpawnTimerDialogViewModel.StartTypes.SLAIN_MESSAGE))
            {
                var regex = new Regex(Model.SlainText, RegexOptions.Compiled);
                var match = regex.Match(slainEvent.Victim);

                if (match.Success)
                {
                    // todo - carry Model's warning/end text and TTS settings into the timer objects

                    var spellname = "Feign Death";
                    spells.AllSpells.TryGetValue(spellname, out var spell);
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        GroupName = CustomTimer.CustomerTime,
                        Name = $"Slain Timer: [{slainEvent.Victim}] [{Model.GetNextTimerCounter()}]",
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        TotalDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                        TotalRemainingDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                        UpdatedDateTime = DateTime.Now,
                        ProgressBarColor = Brushes.DarkSeaGreen
                    });
                }
            }
        }

        private void LogEvents_FactionEvent(object sender, FactionEvent factionEvent)
        {
            Debug.WriteLine($"FactionEvent: [{factionEvent.TimeStamp}], Faction group = [{factionEvent.Faction}]");

            if (Model.SpawnTimerEnabled && (Model.StartType == SpawnTimerDialogViewModel.StartTypes.FACTION_MESSAGE))
            {
                var regex = new Regex(Model.FactionText, RegexOptions.Compiled);
                var match = regex.Match(factionEvent.Faction);

                if (match.Success)
                {
                    // todo - carry Model's warning/end text and TTS settings into the timer objects

                    var spellname = "Feign Death";
                    spells.AllSpells.TryGetValue(spellname, out var spell);
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        GroupName = CustomTimer.CustomerTime,
                        Name = $"Faction Timer: [{factionEvent.Faction}] [{Model.GetNextTimerCounter()}]",
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        TotalDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                        TotalRemainingDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                        UpdatedDateTime = DateTime.Now,
                        ProgressBarColor = Brushes.DarkSeaGreen
                    });
                }
            }
        }
    }
}
