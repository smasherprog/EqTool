using Autofac;
using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Linq;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace EQTool.Services.Parsing
{
    //
    // this parser will watch for deaths, and also for deathloop conditions
    //
    // we will generally define a deathloop condition as
    //      1. At least {deathloop_deaths} experienced,
    //      2. in less than {deathloop_seconds} time,
    //      3. while the player is apparently AFK (no signs of life from casting, meleeing, or communicating)
    //
    public class DeathParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        // todo - make these values configurable
        private readonly int deathloop_deaths = 4;
        private readonly int deathloop_seconds = 120;

        // list of death messages
        // this will function as a scrolling queue, with the oldest message at position 0,
        // newest appended to the other end.  Older messages scroll off the list when more
        // than deathloop_seconds have elapsed.  The list is also flushed any time
        // player activity is detected (i.e. player is not AFK).
        //
        // if/when the length of this list meets or exceeds deathloop_deaths, then
        // the deathloop response is triggered
        private System.Collections.Generic.List<DateTime> deathloop_timestamps = new System.Collections.Generic.List<DateTime>();

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
            // are any of the existing death timestamps too old?
            // if so, remove them from the tracking list
            purge_old_deaths(timestamp);

            // are we apparently AFK?
            // if we are not AFK, then we aren't deathlooping, so purge the entire death tracking list
            check_not_afk(line);

            // have we died?
            // if so, add the death timestamp to the tracking list, and return true
            bool rv = check_for_death(line, timestamp);

            // perform deathloop response
            // if the quantity of deaths in the tracking list exceeds the threshold, then respond appropriately
            if (rv)
            {
                deathloop_response();
            }

            return rv;
        }

        //
        // purge old death timestamps
        //
        private void purge_old_deaths(DateTime timestamp)
        {
            // purge the list of old datetime stamps
            if (deathloop_timestamps.Count > 0)
            {
                bool done = false;
                while (!done)
                {
                    if (deathloop_timestamps.Count == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        // the list of death timestamps has the oldest at position 0
                        DateTime oldest_timestamp = deathloop_timestamps[0];
                        var elapsed_seconds = (timestamp - oldest_timestamp).TotalSeconds;

                        // too much time?
                        if (elapsed_seconds > deathloop_seconds)
                        {
                            // that death is too old, purge it
                            deathloop_timestamps.RemoveAt(0);
                            write_death_times();
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
        // check for signs of player life, i.e. casting, or meleeing, or communicating
        //
        private void check_not_afk(string line)
        {
            // only do the proof of life checks if there are already some death timestamps in the list, else skip this
            if (deathloop_timestamps.Count > 0)
            {
                // check for proof of life, things that indicate the player is not actually AFK
                // begin by assuming the player is AFK
                bool afk = true;

                // does this line contain a proof of life - casting
                string casting_pattern = "^You begin casting";
                Regex casting_regex = new Regex(casting_pattern, RegexOptions.Compiled);
                var match = casting_regex.Match(line);
                if (match.Success)
                {
                    // player is not AFK
                    afk = false;
                    Console.WriteLine($"Player not AFK: [{line}]");
                }

                // does this line contain a proof of life - communications
                //
                // todo - need player name here for that very infrequent use case where tells show up as {playername} ->
                //
                // get player name via dependency injection pattern for ActivePlayer
                // currently failing, throwing exception on null reference?

                //var active_player = container.Resolve<ActivePlayer>();
                //Console.WriteLine($"active_player [{active_player}]");

                //var player = active_player.Player;
                //Console.WriteLine($"player [{player}]");

                //var playername = player.Name;
                //Console.WriteLine($"playername [{playername}]");

                string playername = "Unknown";

                string comm_pattern = $"^(You told|You say|You tell|You auction|You shout|{playername} ->)";
                Regex comm_regex = new Regex(comm_pattern, RegexOptions.Compiled);
                match = comm_regex.Match(line);
                if (match.Success)
                {
                    // player is not AFK
                    afk = false;
                    Console.WriteLine($"Player not AFK: [{line}]");
                }

                // does this line contain a proof of life - melee
                string melee_pattern = "^You( try to)? (hit|slash|pierce|crush|claw|bite|sting|maul|gore|punch|kick|backstab|bash)";
                Regex melee_regex = new Regex(melee_pattern, RegexOptions.Compiled);
                match = melee_regex.Match(line);
                if (match.Success)
                {
                    // player is not AFK
                    afk = false;
                    Console.WriteLine($"Player not AFK: [{line}]");
                }

                // if player is not AFK, then purge the death list
                if (!afk)
                {
                    deathloop_timestamps.Clear();
                    write_death_times();
                }
            }
        }

        //
        // check to see if the current line indicates the player has died
        //
        private bool check_for_death(string line, DateTime timestamp)
        {
            // return value
            bool rv = false;

            // this regex allows the parser to watch for the real phrase, but also to be tested by
            // sending a tell while in-game to the non-existent user ".death"
            string death_pattern = @"(^\.death )|(^You have been slain)";
            Regex death_regex = new Regex(death_pattern, RegexOptions.Compiled);
            var match = death_regex.Match(line);

            // if we have died
            if (match.Success)
            {
                // handle the event
                logEvents.Handle(new DeathEvent());

                // todo - we have died, so strip away all buff timers

                // add this timestamp to the end of the tracking list, and print a debug message
                deathloop_timestamps.Add(timestamp);
                write_death_times();

                rv = true;
            }

            return rv;
        }


        //
        // response if a deathloop condition is detected
        // an ideal solution would be to kill the eqgame.exe process, however
        // that is currently ruled out of bounds, so we will have to do something more benign for now
        //
        private void deathloop_response()
        {
            // deathloop response
            if (deathloop_timestamps.Count >= deathloop_deaths)
            {
                // just a little audible marker to help us debug and test
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                player.Play();

                // since we can't kill eqgame.exe, try to alert the user by yelling at him
                SpeechSynthesizer synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
                synth.Rate = 2;
                synth.Volume = 100; // 0-100, as loud as we can
                synth.Speak("death loop, death loop, death loop");

                // write some debug lines
                Console.WriteLine("------------------------------------Deathloop condition detected!-----------------------------------------");
                Console.WriteLine($"{deathloop_deaths} or more deaths in less than {deathloop_seconds} seconds, with no player activity");
                write_death_times();
                Console.WriteLine("We really should be killing the eqgame.exe process right now");
                Console.WriteLine("------------------------------------Deathloop condition detected!-----------------------------------------");
            }

        }

        //
        // utility function to print the contents of the death timestamp list
        //
        private void write_death_times()
        {
            // print a list of timestamps
            Console.Write($"Death timestamps: count = {deathloop_timestamps.Count}, times = ");
            for (int i = 0; i < deathloop_timestamps.Count; i++)
            {
                Console.Write($"[{deathloop_timestamps[i]}] ");
            }
            Console.WriteLine();
        }

    }
}
