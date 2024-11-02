using EQTool.Models;
using EQTool.ViewModels;
using System.Diagnostics;

namespace EQTool.Services.Handlers
{
    //
    // class to create spawn timers
    //
    // watches for ExpGainedEvent, FactionEvent, and DeathEvent types
    //
    internal class SpawnTimerHandler : BaseHandler
    {
        //
        // ctor
        //
        // register this service as a listener for the Events it cares about
        //
        public SpawnTimerHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.ExpGainedEvent += LogEvents_ExpGainedEvent;
            this.logEvents.SlainEvent += LogEvents_SlainEvent;
            this.logEvents.FactionEvent += LogEvents_FactionEvent;
        }


        //
        // function that gets called for a ExpGainedEvent
        //
        private void LogEvents_ExpGainedEvent(object sender, ExpGainedEvent expGainedEvent)
        {
            // debugging message
            Debug.WriteLine($"ExpGainedEvent: [{expGainedEvent.TimeStamp}] [{expGainedEvent.Line}]");

            // react


        }

        //
        // function that gets called for a SlainhEvent
        //
        private void LogEvents_SlainEvent(object sender, SlainEvent slainEvent)
        {
            // debugging message
            Debug.WriteLine($"SlainEvent: [{slainEvent.TimeStamp}], Killer = [{slainEvent.Killer}], Victim = [{slainEvent.Victim}]");

            // if the victim field matches to the SpawnTimer field, then react


        }

        //
        // function that gets called for a FactionEvent
        //
        private void LogEvents_FactionEvent(object sender, FactionEvent factionEvent)
        {
            // debugging message
            Debug.WriteLine($"FactionEvent: [{factionEvent.TimeStamp}], Faction group = [{factionEvent.Faction}]");

            // if the faction field matches to the SpawnTimer field, then react


        }

    }

    //
    // class to hold spawn trigger info gathered from UI
    //
    public class SpawnTimerTrigger
    {
        //
        // top level is-enabled flag
        //
        public bool SpawnTimerEnabled { get; set; }

        //
        // timer start fields
        //
        public enum StartTypes
        {
            AI,
            EXP_MESSAGE,
            SLAIN_MESSAGE,
            FACTION_MESSAGE
        }
        public SpawnTimerTrigger.StartTypes StartType { get; set; }

        //
        // timer duration fields
        //
        public enum Durations
        {
            PRESET_0600,
            PRESET_0640,
            PRESET_1430,
            PRESET_2200,
            PRESET_2800,
            CUSTOM
        }
        public SpawnTimerTrigger.Durations Duration { get; set; }
        public string CustomDuration { get; set; }

        //
        // timer end fields
        //

        // timer expiring soon warnings
        public string WarningTime { get; set; }
        public bool ProvideWarningText { get; set; }
        public bool ProvideWarningTTS { get; set; }
        public string WarningText { get; set; }
        public string WarningTTS { get; set; }

        // timer expired notifications
        public bool ProvideEndText { get; set; }
        public bool ProvideEndTTS { get; set; }
        public string EndText { get; set; }
        public string EndTTS { get; set; }

        //
        // counter reset field
        //
        public string CounterResetTime { get; set; }

    }

}
