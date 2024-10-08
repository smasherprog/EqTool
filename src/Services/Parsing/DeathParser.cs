using EQTool.Models;
using System;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    public class DeathParser : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        public DeathParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        public bool Handle(string line, DateTime timestamp)
        {
            // this regex allows the parser to watch for the real phrase, but also to be tested by
            // sending a tell while in-game to the non-existent user ".death "
            string pattern = @"(^\.death )|(^You have been slain)";
            Regex regex = new Regex(pattern, RegexOptions.Compiled);
            var match = regex.Match(line);

            if (match.Success)
            {
                // handle the event
                logEvents.Handle(new DeathEvent());

                // just a little audible marker to help us debug and test
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                player.Play();

                // we have died, so strip away all buff timers
                // todo

                // check for deathloop conditions
                // todo

                return true;
            }
            return false;
        }

    }
}
