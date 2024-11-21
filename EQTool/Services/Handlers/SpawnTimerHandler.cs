using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Diagnostics;
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
        public SpawnTimerHandler(
            LogEvents logEvents, 
            SpellWindowViewModel spellWindowViewModel,
            EQSpells spells,
            ActivePlayer activePlayer, 
            EQToolSettings eQToolSettings, 
            ITextToSpeach textToSpeach)
            : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
            this.logEvents.ExpGainedEvent += LogEvents_ExpGainedEvent;
            this.logEvents.SlainEvent += LogEvents_SlainEvent;
            this.logEvents.FactionEvent += LogEvents_FactionEvent;
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
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
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

                    var spellname = "Feign Death";
                    var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
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
                    var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
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
