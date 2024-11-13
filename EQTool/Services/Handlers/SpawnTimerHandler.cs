using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.HubModels;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EQTool.Services.Handlers
{
    //
    // class to create spawn timers
    //
    // watches for ExpGainedEvent, FactionEvent, and DeathEvent types
    //
    public class SpawnTimerHandler : BaseHandler
    {
        // Model class to hold the results of the Spawn Timer Dialog
        private readonly SpawnTimerDialogViewModel _model = new SpawnTimerDialogViewModel();

        //
        // ctor
        //
        // register this service as a listener for the Events it cares about
        //
        public SpawnTimerHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach)
            : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.ExpGainedEvent += LogEvents_ExpGainedEvent;
            this.logEvents.SlainEvent += LogEvents_SlainEvent;
            this.logEvents.FactionEvent += LogEvents_FactionEvent;
        }

        // getter for the spawn timer Model
        public SpawnTimerDialogViewModel Model => _model;

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
                // fire off a timer event
                var timer = new StartTimerEvent
                {
                    CustomTimer = new CustomTimer
                    {
                        DurationSeconds             = Model.DurationSeconds,
                        Name                        = $"Exp Timer [{Model.GetNextTimerCounter()}]",
                        WarningSeconds              = Model.WarningSeconds,
                        ProvideWarningText          = Model.ProvideWarningText,
                        ProvideWarningTTS           = Model.ProvideWarningTTS,
                        WarningText                 = Model.WarningText,
                        WarningTTS                  = Model.WarningTTS,
                        ProvideEndText              = Model.ProvideEndText,
                        ProvideEndTTS               = Model.ProvideEndTTS,
                        EndText                     = Model.EndText,
                        EndTTS                      = Model.EndTTS,
                        RestartExisting             = false,
                    },

                    Line                            = expGainedEvent.Line,
                    TimeStamp                       = expGainedEvent.TimeStamp
                };
                logEvents.Handle(timer);
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
                Regex regex = new Regex(Model.SlainText, RegexOptions.Compiled);
                var match = regex.Match(slainEvent.Victim);

                if (match.Success)
                {
                    // fire off a timer event
                    var timer = new StartTimerEvent
                    {
                        CustomTimer = new CustomTimer
                        {
                            DurationSeconds         = Model.DurationSeconds,
                            Name                    = $"Slain Timer: [{slainEvent.Victim}] [{Model.GetNextTimerCounter()}]",
                            WarningSeconds          = Model.WarningSeconds,
                            ProvideWarningText      = Model.ProvideWarningText,
                            ProvideWarningTTS       = Model.ProvideWarningTTS,
                            WarningText             = Model.WarningText,
                            WarningTTS              = Model.WarningTTS,
                            ProvideEndText          = Model.ProvideEndText,
                            ProvideEndTTS           = Model.ProvideEndTTS,
                            EndText                 = Model.EndText,
                            EndTTS                  = Model.EndTTS,
                            RestartExisting         = false,
                        },

                        Line                        = slainEvent.Line,
                        TimeStamp                   = slainEvent.TimeStamp
                    };

                    logEvents.Handle(timer);
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
                Regex regex = new Regex(Model.FactionText, RegexOptions.Compiled);
                var match = regex.Match(factionEvent.Faction);

                if (match.Success)
                {
                    // fire off a timer event
                    var timer = new StartTimerEvent
                    {
                        CustomTimer = new CustomTimer
                        {
                            DurationSeconds         = Model.DurationSeconds,
                            Name                    = $"Faction Timer: [{factionEvent.Faction}] [{Model.GetNextTimerCounter()}]",
                            WarningSeconds          = Model.WarningSeconds,
                            ProvideWarningText      = Model.ProvideWarningText,
                            ProvideWarningTTS       = Model.ProvideWarningTTS,
                            WarningText             = Model.WarningText,
                            WarningTTS              = Model.WarningTTS,
                            ProvideEndText          = Model.ProvideEndText,
                            ProvideEndTTS           = Model.ProvideEndTTS,
                            EndText                 = Model.EndText,
                            EndTTS                  = Model.EndTTS,
                            RestartExisting         = false,
                        },

                        Line                        = factionEvent.Line,
                        TimeStamp                   = factionEvent.TimeStamp
                    };

                    logEvents.Handle(timer);
                }
            }
        }
    }
}
