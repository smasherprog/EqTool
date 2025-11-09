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
    //
    // class to create spawn timers
    //
    // watches for ExpGainedEvent, FactionEvent, and DeathEvent types
    //
    public class SpawnTimerHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        //
        // ctor
        //
        // register this service as a listener for the Events it cares about
        //
        public SpawnTimerHandler(SpellWindowViewModel spellWindowViewModel, EQSpells spells, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
            logEvents.ExpGainedEvent += LogEvents_ExpGainedEvent;
            logEvents.SlainEvent += LogEvents_SlainEvent;
            logEvents.FactionEvent += LogEvents_FactionEvent;
        }

        // getter for the spawn timer Model
        public SpawnTimerDialogViewModel Model { get; } = new SpawnTimerDialogViewModel();

        //
        // function that gets called for a ExpGainedEvent
        //
        private void LogEvents_ExpGainedEvent(object sender, ExpGainedEvent expGainedEvent)
        {
            // debugging message
            Debug.WriteLine($"ExpGainedEvent: [{expGainedEvent.TimeStamp}] [{expGainedEvent.Line}]");

            // are spawn timers for exp messages turned on?
            if (Model.SpawnTimerEnabled && (Model.StartType == SpawnTimerDialogViewModel.StartTypes.EXP_MESSAGE))
            {
                // todo - incorporate all tehse features into the timer objects
                //
                //        WarningSeconds = Model.WarningSeconds,
                //        ProvideWarningText = Model.ProvideWarningText,
                //        ProvideWarningTTS = Model.ProvideWarningTTS,
                //        WarningText = Model.WarningText,
                //        WarningTTS = Model.WarningTTS,
                //        ProvideEndText = Model.ProvideEndText,
                //        ProvideEndTTS = Model.ProvideEndTTS,
                //        EndText = Model.EndText,
                //        EndTTS = Model.EndTTS,

                var spellname = "Feign Death";
                spells.AllSpells.TryGetValue(spellname, out var timerVisuals);
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    Target = CustomTimer.CustomerTime,
                    Id = $"Exp Timer [{Model.GetNextTimerCounter()}]",
                    Rect = timerVisuals.Rect,
                    Icon = timerVisuals.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                    TotalRemainingDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.DarkSeaGreen
                });
            }
        }

        //
        // function that gets called for a SlainhEvent
        //
        private void LogEvents_SlainEvent(object sender, SlainEvent slainEvent)
        {
            // debugging message
            Debug.WriteLine($"SlainEvent: [{slainEvent.TimeStamp}], Killer = [{slainEvent.Killer}], Victim = [{slainEvent.Victim}]");

            // if the victim field matches to the SpawnTimer field, then react
            // are spawn timers for faction messages turned on?
            if (Model.SpawnTimerEnabled && (Model.StartType == SpawnTimerDialogViewModel.StartTypes.SLAIN_MESSAGE))
            {
                // does this faction match the user-specified factions?
                var regex = new Regex(Model.SlainText, RegexOptions.Compiled);
                var match = regex.Match(slainEvent.Victim);

                if (match.Success)
                {
                    // todo - incorporate all tehse features into the timer objects
                    //
                    //        WarningSeconds = Model.WarningSeconds,
                    //        ProvideWarningText = Model.ProvideWarningText,
                    //        ProvideWarningTTS = Model.ProvideWarningTTS,
                    //        WarningText = Model.WarningText,
                    //        WarningTTS = Model.WarningTTS,
                    //        ProvideEndText = Model.ProvideEndText,
                    //        ProvideEndTTS = Model.ProvideEndTTS,
                    //        EndText = Model.EndText,
                    //        EndTTS = Model.EndTTS,

                    spells.AllSpells.TryGetValue("Feign Death", out var timerVisuals);
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        Target = CustomTimer.CustomerTime,
                        Id = $"Slain Timer: [{slainEvent.Victim}] [{Model.GetNextTimerCounter()}]",
                        Rect = timerVisuals.Rect,
                        Icon = timerVisuals.SpellIcon,
                        TotalDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                        TotalRemainingDuration = TimeSpan.FromSeconds(Model.DurationSeconds),
                        UpdatedDateTime = DateTime.Now,
                        ProgressBarColor = Brushes.DarkSeaGreen
                    });
                }
            }
        }

        //
        // function that gets called for a FactionEvent
        //
        private void LogEvents_FactionEvent(object sender, FactionEvent factionEvent)
        {
            // debugging message
            Debug.WriteLine($"FactionEvent: [{factionEvent.TimeStamp}], Faction group = [{factionEvent.Faction}]");

            // are spawn timers for faction messages turned on?
            if (Model.SpawnTimerEnabled && (Model.StartType == SpawnTimerDialogViewModel.StartTypes.FACTION_MESSAGE))
            {
                // does this faction match the user-specified factions?
                var regex = new Regex(Model.FactionText, RegexOptions.Compiled);
                var match = regex.Match(factionEvent.Faction);

                if (match.Success)
                {
                    // todo - incorporate all tehse features into the timer objects
                    //
                    //        WarningSeconds = Model.WarningSeconds,
                    //        ProvideWarningText = Model.ProvideWarningText,
                    //        ProvideWarningTTS = Model.ProvideWarningTTS,
                    //        WarningText = Model.WarningText,
                    //        WarningTTS = Model.WarningTTS,
                    //        ProvideEndText = Model.ProvideEndText,
                    //        ProvideEndTTS = Model.ProvideEndTTS,
                    //        EndText = Model.EndText,
                    //        EndTTS = Model.EndTTS,

                    var spellname = "Feign Death";
                    spells.AllSpells.TryGetValue(spellname, out var timerVisuals);
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        Target = CustomTimer.CustomerTime,
                        Id = $"Faction Timer: [{factionEvent.Faction}] [{Model.GetNextTimerCounter()}]",
                        Rect = timerVisuals.Rect,
                        Icon = timerVisuals.SpellIcon,
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
