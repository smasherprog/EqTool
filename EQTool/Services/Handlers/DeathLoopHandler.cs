using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Diagnostics;

namespace EQTool.Services.Handlers
{

    //
    // this Service class (Event listener) will watch for deaths, and monitor for deathloop conditions
    //
    // we will generally define a deathloop condition as
    //      1. At least {_deathLoopDeaths} experienced,
    //      2. in less than {_deathLoopSeconds} time,
    //      3. while the player is apparently AFK (no signs of life from casting, meleeing, or communicating)
    //
    // it listens for the Events created by other Parser classes.
    //      DeathEvent
    //      DamageEvent
    //      YouBeginCastingEvent
    //      CommsEvent
    //
    public class DeathLoopHandler : BaseHandler
    {
        // todo - make these values configurable
        private readonly int _deathLoopDeaths = 4;
        private readonly int _deathLoopSeconds = 120;

        // list of death timestamps
        // this will function as a scrolling queue, with the oldest message at position 0,
        // newest appended to the other end.  Older messages scroll off the list when more
        // than _deathLoopSeconds have elapsed.  The list is also flushed any time
        // player activity is detected (i.deathEvent. player is not AFK).
        //
        // if/when the length of this list meets or exceeds _deathLoopDeaths, then
        // the deathloop response is triggered
        private readonly System.Collections.Generic.List<DateTime> _deathLoopTimeStamps = new System.Collections.Generic.List<DateTime>();

        //
        // ctor
        //
        // register this service as a listener for the Events it cares about
        //
        public DeathLoopHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.DeathEvent += LogEvents_DeathEvent;
            this.logEvents.DamageEvent += LogEvents_DamageEvent;
            this.logEvents.YouBeginCastingEvent += LogEvents_YouBeginCastingEvent;
            this.logEvents.CommsEvent += LogEvents_CommsEvent;
        }

        public bool IsDeathLooping()
        {
            return _deathLoopTimeStamps.Count >= _deathLoopDeaths;
        }

        public int DeathCount()
        {
            return _deathLoopTimeStamps.Count;
        }


        //
        // utility function to print the contents of the death timestamp list
        //
        private void WriteDeathTimes()
        {
            // print a list of timestamps
            Debug.WriteLine($"Death timestamps: count = {_deathLoopTimeStamps.Count}, times = ");
            for (var i = 0; i < _deathLoopTimeStamps.Count; i++)
            {
                Debug.Write($"[{_deathLoopTimeStamps[i]}] ");
            }

            Debug.WriteLine(string.Empty);
        }

        //
        // function that gets called for a DeathEvent
        //
        private void LogEvents_DeathEvent(object sender, DeathEvent deathEvent)
        {
            // use current time to see if any death time stamps in the list need to roll off
            UpdateDeathList(deathEvent.TimeStamp);

            // did player just die?
            if (deathEvent.Victim == "You")
            {
                // add this timestamp to the end of the tracking list, and print a debug message
                _deathLoopTimeStamps.Add(deathEvent.TimeStamp);
                WriteDeathTimes();

                // if the quantity of deaths in the tracking list exceeds the threshold, then respond appropriately
                if (_deathLoopTimeStamps.Count >= _deathLoopDeaths)
                {
                    DeathLoopResponse(deathEvent.TimeStamp, deathEvent.Line);
                }
            }
        }

        // walk the list of death times, and see if any need to roll off
        private void UpdateDeathList(DateTime currentTimeStamp)
        {
            // begin by walking the list and purge the old datetime stamps
            if (_deathLoopTimeStamps.Count > 0)
            {
                var done = false;
                while (!done)
                {
                    if (_deathLoopTimeStamps.Count == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        // the list of death timestamps has the oldest at position 0
                        var oldestTimestamp = _deathLoopTimeStamps[0];
                        var elapsedSeconds = (currentTimeStamp - oldestTimestamp).TotalSeconds;

                        // too much time?
                        if (elapsedSeconds > _deathLoopSeconds)
                        {
                            // that death is too old, purge it
                            _deathLoopTimeStamps.RemoveAt(0);
                            WriteDeathTimes();
                        }
                        else
                        {
                            // the oldest death time is inside the window, so we're done purging
                            done = true;
                        }
                    }
                }
            }
        }

        //
        // response for when a death loop condition is detected
        //
        public void DeathLoopResponse(DateTime timestamp, string line)
        {
            // since we can't kill eqgame.exe, try to alert the user by yelling at him/her
            // fire an event to the AudioService for it to respond to 
            textToSpeach.Say("death loop death loop death loop. death loop!");

            //var synth = new SpeechSynthesizer();
            //synth.SetOutputToDefaultAudioDevice();
            //synth.Rate = 2;
            //synth.Volume = 100; // 0-100, as loud as we can
            //synth.Speak("death loop, death loop, death loop");

            // write some debug lines
            Console.WriteLine("------------------------------------Deathloop condition detected!-----------------------------------------");
            Console.WriteLine($"{_deathLoopDeaths} or more deaths in less than {_deathLoopSeconds} seconds, with no player activity");
            WriteDeathTimes();
            Console.WriteLine("We really should be killing the eqgame.exe process right now");
            Console.WriteLine("------------------------------------Deathloop condition detected!-----------------------------------------");
        }

        //
        // function that gets called for a DamgeEvent
        //
        private void LogEvents_DamageEvent(object sender, DamageEvent damageEvent)
        {
            // use current time to see if any death time stamps in the list need to roll off
            UpdateDeathList(damageEvent.TimeStamp);

            // if the player is meleeing, then flush to death list
            if (damageEvent.AttackerName == "You")
            {
                // player is engaging in melee, i.deathEvent. not AFK, so clear death list
                _deathLoopTimeStamps.Clear();
            }
        }

        //
        // function that gets called for a YouBeginCastingEvent
        //
        private void LogEvents_YouBeginCastingEvent(object sender, YouBeginCastingEvent youBeginCastingEvent)
        {
            // player is casting, i.deathEvent. not AFK, so clear death list
            _deathLoopTimeStamps.Clear();
        }


        //
        // function that gets called for a CommsEvent
        //
        private void LogEvents_CommsEvent(object sender, CommsEvent commsEvent)
        {
            // use current time to see if any death time stamps in the list need to roll off
            UpdateDeathList(commsEvent.TimeStamp);

            // a comms event in any channel from the player indicates the player is active
            if ((commsEvent.TheChannel != CommsEvent.Channel.NONE) && (commsEvent.Sender == "You"))
            {
                // player is communicating, i.deathEvent. not AFK, so clear death list
                _deathLoopTimeStamps.Clear();
            }
        }
    }
}
