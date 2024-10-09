using EQTool.Models;
using System;
using System.Linq;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using static EQTool.Services.Parsing.InvisParser;

namespace EQTool.Services.Parsing
{
    public class DeathParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        // we will generally define a deathloop condition as
        // - At least 'deathloop_deaths' experienced,
        // - In 'deathloop_seconds' time,
        // - while the player is apparently AFK (i.e. no signs of life, such as casting, meleeing, or communicating)

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
            // have we died?
            check_for_death(line, timestamp);

            // are we apparently AFK?
            check_not_afk(line, timestamp);

            // perform deathloop response
            deathloop_response();

            // todo - what is a sensible return value
            return false;
        }

        //
        // check to see if the current line indicates the player has died
        //
        public void check_for_death(string line, DateTime timestamp)
        {

            // this regex allows the parser to watch for the real phrase, but also to be tested by
            // sending a tell while in-game to the non-existent user ".death "
            string death_pattern = @"(^\.death )|(^You have been slain)";
            Regex death_regex = new Regex(death_pattern, RegexOptions.Compiled);
            var match = death_regex.Match(line);

            // if we have died
            if (match.Success)
            {
                // handle the event
                logEvents.Handle(new DeathEvent());

                //// just a little audible marker to help us debug and test
                //System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                //player.Play();

                // we have died, so strip away all buff timers
                // todo

                // add this timestamp to the end of the tracking list, and print a debug message
                deathloop_timestamps.Add(timestamp);
                write_death_times();
            }

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
        public void check_not_afk(string line, DateTime timestamp)
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
                // todo - need player name here
                string playername = "ToBeDetermined";
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
        // response if a deathloop condition is detected
        // an ideal solution would be to kill the eqgame.exe process, however
        // that is currently ruled out of bounds, so we will have to do something more benign for now
        //
        public void deathloop_response()
        {
            // deathloop response
            if (deathloop_timestamps.Count >= deathloop_deaths)
            {
                // just a little audible marker to help us debug and test
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                player.Play();

                // todo - how to alert player 
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
            // any timestamps in list?
            if (deathloop_timestamps.Count > 0)
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
}
