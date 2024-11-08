using EQTool.Models;
using EQTool.UI;
using EQTool.ViewModels;
using System.Diagnostics;

namespace EQTool.Services.Handlers
{
    //
    // class to create spawn timers
    //
    // watches for ExpGainedEvent, FactionEvent, and DeathEvent types
    //
    //internal class SpawnTimerHandler : BaseHandler
    public class SpawnTimerHandler : BaseHandler
    {
        // Model class to hold the results of the Spawn Timer Dialog
        // make this static, so it only initializes once
        static private readonly SpawnTimerTrigger _trigger = new SpawnTimerTrigger();

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

        // getter for the spawn timer trigger
        public SpawnTimerTrigger Trigger { get { return _trigger; } }


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
        // utility function to set all fields to the corresponding field in the ViewModel class
        // todo - instead of laboriously converting between the VM and the M, can we just use same class for both?
        public void SetFrom(SpawnTimerDialogViewModel vm)
        {
            // sweep thru all the fields

            //
            // overall enable/disable
            //
            SpawnTimerEnabled = vm.SpawnTimerEnabled;

            //
            // timer start
            //

            // vm doesn't know about enums
            if (vm.PigParseAI)
                StartType = StartTypes.PIG_PARSE_AI;
            else if (vm.ExpMessage)
                StartType = StartTypes.EXP_MESSAGE;
            else if (vm.SlainMessage)
                StartType = StartTypes.SLAIN_MESSAGE;
            else if (vm.FactionMessage)
                StartType = StartTypes.FACTION_MESSAGE;

            SlainText = vm.SlainText;
            FactionText = vm.FactionText;

            //
            // timer end
            //
            WarningTime = vm.WarningTime;

            ProvideWarningText = vm.ProvideWarningText;
            ProvideWarningTTS = vm.ProvideWarningTTS;
            WarningText = vm.WarningText;
            WarningTTS = vm.WarningTTS;

            ProvideEndText = vm.ProvideEndText;
            ProvideEndTTS = vm.ProvideEndTTS;
            EndText = vm.EndText;
            EndTTS = vm.EndTTS;

            //
            // counter reset field
            //
            CounterResetTime = vm.CounterResetTime;

            //
            // timer duration fields
            //

            // vm doesn't know about enums
            if (vm.Preset0600)
                Duration = Durations.PRESET_0600;
            else if (vm.Preset0640)
                Duration = Durations.PRESET_0640;
            else if (vm.Preset1430)
                Duration = Durations.PRESET_1430;
            else if (vm.Preset2200)
                Duration = Durations.PRESET_2200;
            else if (vm.Preset2800)
                Duration = Durations.PRESET_2800;
            else if (vm.Custom)
                Duration = Durations.CUSTOM;

            CustomDuration = vm.CustomDuration;

            //
            // notes and comments field
            //
            // todo - add in the proper field for a RichTextBox


        }



        //
        // top level is-enabled flag
        //
        public bool SpawnTimerEnabled { get; set; } = false;

        //
        // timer start fields
        //
        public enum StartTypes
        {
            PIG_PARSE_AI,
            EXP_MESSAGE,
            SLAIN_MESSAGE,
            FACTION_MESSAGE
        }
        public SpawnTimerTrigger.StartTypes StartType { get; set; } = StartTypes.EXP_MESSAGE;

        public string SlainText { get; set; } = "(an ancient cyclops|a pirate|a cyclops|Boog Mudtoe)";
        public string FactionText { get; set; } = "(Coldain|Rygorr)";

        //
        // timer end fields
        //

        // timer expiring soon warnings
        public string WarningTime { get; set; } = "30";
        public bool ProvideWarningText { get; set; } = true;
        public bool ProvideWarningTTS { get; set; } = true;
        public string WarningText { get; set; } = "30 second warning";
        public string WarningTTS { get; set; } = "30 second warning";

        // timer expired notifications
        public bool ProvideEndText { get; set; } = true;
        public bool ProvideEndTTS { get; set; } = true;
        public string EndText { get; set; } = "Pop";
        public string EndTTS { get; set; } = "Pop";

        //
        // counter reset field
        //
        public string CounterResetTime { get; set; } = "1:00:00";

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
        public SpawnTimerTrigger.Durations Duration { get; set; } = Durations.CUSTOM;
        public string CustomDuration { get; set; } = "30:00";

        //
        // notes and comments field
        //
        // todo - add in the proper field for a RichTextBox



    }

}
