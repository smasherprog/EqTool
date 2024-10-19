using System;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    //
    // this parser will watch for deaths, and also for deathloop conditions
    //
    // we will generally define a deathloop condition as
    //      1. At least {_deathLoopDeaths} experienced,
    //      2. in less than {_deathLoopSeconds} time,
    //      3. while the player is apparently AFK (no signs of life from casting, meleeing, or communicating)
    //
    public class DeathParser
    {
        private readonly LogEvents logEvents;

        // todo - make these values configurable
        private readonly int _deathLoopDeaths = 4;
        private readonly int _deathLoopSeconds = 120;

        // list of death messages
        // this will function as a scrolling queue, with the oldest message at position 0,
        // newest appended to the other end.  Older messages scroll off the list when more
        // than _deathLoopSeconds have elapsed.  The list is also flushed any time
        // player activity is detected (i.e. player is not AFK).
        //
        // if/when the length of this list meets or exceeds _deathLoopDeaths, then
        // the deathloop response is triggered
        private readonly System.Collections.Generic.List<DateTime> _deathLoopTimestamps = new System.Collections.Generic.List<DateTime>();

        //
        // ctor
        //
        public DeathParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        //
        // overload the Handle method to perform the specific parsing
        //
        public bool Handle(string line, DateTime timestamp)
        {
            // are we apparently AFK?
            // if we are not AFK, then we aren't deathlooping, so purge the entire death tracking list
            _ = ParseSignOfLife(line);

            // have we died?
            // if so, add the death timestamp to the tracking list, and return true
            var rv = ParseDeath(line, timestamp);

            // perform deathloop response
            // if the quantity of deaths in the tracking list exceeds the threshold, then respond appropriately
            if (rv)
            {
                _ = DeathLoopResponse();
            }

            return rv;
        }

        //
        // check if a player is showing "signs of life" and is therefore not AFK
        //
        // return:
        //      true    there is already at least 1 death being tracked in the tracking list, AND
        //              line contains evidence of signs of life.  Player is casting, communicating, or meleeing.
        //      false   otherwise
        //
        public bool ParseSignOfLife(string line)
        {
            // check for proof of life, things that indicate the player is not actually AFK
            // begin by assuming the player is AFK
            var signOfLife = false;

            // only do the proof of life checks if there are already some death timestamps in the list, else skip this
            if (_deathLoopTimestamps.Count > 0)
            {
                // does this line contain a proof of life - casting
                var castingPattern = "^You begin casting";
                var castingRegex = new Regex(castingPattern, RegexOptions.Compiled);
                var match = castingRegex.Match(line);
                if (match.Success)
                {
                    // player is not AFK
                    signOfLife = true;
                    Console.WriteLine($"Player not AFK: [{line}]");
                }

                // does this line contain a proof of life - communications
                //
                // todo - need player name here for that very infrequent use case where tells show up as {playerName} ->
                //
                // get player name via dependency injection pattern for ActivePlayer
                //
                // where does container live?
                //var activePlayer = container.Resolve<ActivePlayer>();
                //string playerName = activePlayer.Player.Name;
                var playerName = "Unknown";

                var commsPattern = $"^(You told|You say|You tell|You auction|You shout|{playerName} ->)";
                var commsRegex = new Regex(commsPattern, RegexOptions.Compiled);
                match = commsRegex.Match(line);
                if (match.Success)
                {
                    // player is not AFK
                    signOfLife = true;
                    Console.WriteLine($"Player not AFK: [{line}]");
                }

                // does this line contain a proof of life - melee
                var meleePattern = "^You( try to)? (hit|slash|pierce|crush|claw|bite|sting|maul|gore|punch|kick|backstab|bash|slice|strike)";
                var meleeRegex = new Regex(meleePattern, RegexOptions.Compiled);
                match = meleeRegex.Match(line);
                if (match.Success)
                {
                    // player is not AFK
                    signOfLife = true;
                    Console.WriteLine($"Player not AFK: [{line}]");
                }

                // if player is not AFK, then purge the death list
                if (signOfLife)
                {
                    _deathLoopTimestamps.Clear();
                    writeDeathTimes();
                }
            }

            return signOfLife;
        }

        //
        // check to see if the current line indicates the player has died
        //
        // return:
        //      true    line indicates that player has died
        //      false   otherwise
        //
        public bool ParseDeath(string line, DateTime timestamp)
        {
            // are any of the existing death timestamps too old?
            // if so, remove them from the tracking list
            purgeOldDeaths(timestamp);

            // return value
            var rv = false;

            // this regex allows the parser to watch for the real phrase, but also to be tested by
            // sending a tell while in-game to the non-existent user ".death"
            var deathPattern = @"(^\.death )|(^You have been slain)";
            var deathRegex = new Regex(deathPattern, RegexOptions.Compiled);
            var match = deathRegex.Match(line);

            // if we have died
            if (match.Success)
            {
                // handle the event
                // logEvents.Handle(new DeathEvent());

                // todo - we have died, so strip away all buff timers

                // add this timestamp to the end of the tracking list, and print a debug message
                _deathLoopTimestamps.Add(timestamp);
                writeDeathTimes();

                rv = true;
            }

            return rv;
        }

        //
        // response if a deathloop condition is detected
        // an ideal solution would be to kill the eqgame.exe process, however
        // that is currently ruled out of bounds, so we will have to do something more benign for now
        //
        // returns the number of death timestamps currently in the tracking list
        //
        public int DeathLoopResponse()
        {
            // deathloop response
            if (_deathLoopTimestamps.Count >= _deathLoopDeaths)
            {
                // just a little audible marker
                var player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                player.Play();

                // since we can't kill eqgame.exe, try to alert the user by yelling at him
                var synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
                synth.Rate = 2;
                synth.Volume = 100; // 0-100, as loud as we can
                synth.Speak("death loop, death loop, death loop");

                // write some debug lines
                Console.WriteLine("------------------------------------Deathloop condition detected!-----------------------------------------");
                Console.WriteLine($"{_deathLoopDeaths} or more deaths in less than {_deathLoopSeconds} seconds, with no player activity");
                writeDeathTimes();
                Console.WriteLine("We really should be killing the eqgame.exe process right now");
                Console.WriteLine("------------------------------------Deathloop condition detected!-----------------------------------------");
            }

            return _deathLoopTimestamps.Count;
        }

        //
        // purge old death timestamps
        //
        private void purgeOldDeaths(DateTime timestamp)
        {
            // walk the list and purge the old datetime stamps
            if (_deathLoopTimestamps.Count > 0)
            {
                var done = false;
                while (!done)
                {
                    if (_deathLoopTimestamps.Count == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        // the list of death timestamps has the oldest at position 0
                        var oldestTimestamp = _deathLoopTimestamps[0];
                        var elapsedSeconds = (timestamp - oldestTimestamp).TotalSeconds;

                        // too much time?
                        if (elapsedSeconds > _deathLoopSeconds)
                        {
                            // that death is too old, purge it
                            _deathLoopTimestamps.RemoveAt(0);
                            writeDeathTimes();
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
        // utility function to print the contents of the death timestamp list
        //
        private void writeDeathTimes()
        {
            // print a list of timestamps
            Debug.WriteLine($"Death timestamps: count = {_deathLoopTimestamps.Count}, times = ");
            for (var i = 0; i < _deathLoopTimestamps.Count; i++)
            {
                Debug.Write($"[{_deathLoopTimestamps[i]}] ");
            }
            Debug.WriteLine(string.Empty);
        }

    }
}
