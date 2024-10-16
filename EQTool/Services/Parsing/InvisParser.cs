using EQTool.Models;
using System;
using System.Media;
using System.Text.RegularExpressions;


namespace EQTool.Services.Parsing
{
    public class InvisParser : IEqLogParseHandler
    {
        public enum InvisStatus
        {
            Fading,
            Faded,
            Applied
        }
        private readonly LogEvents logEvents;

        public InvisParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }
        public bool Handle(string line, DateTime timestamp)
        {
            var m = Parse(line);
            if (m != null)
            {
                logEvents.Handle(new InvisEvent { InvisStatus = m.Value });

                //// just a little audible marker to help us debug and test
                //System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\chimes.wav");
                //player.Play();

                return true;
            }
            return false;
        }

        public InvisStatus? Parse(string line)
        {
            // this regex allows the parser to watch for the real phrase, but also to be tested by
            // sending a tell while in-game to the non-existent user ".inv "
            string pattern = @"(^\.inv )|(^You feel yourself starting to appear.)";
            Regex regex = new Regex(pattern, RegexOptions.Compiled);
            var match = regex.Match(line);
            return match.Success ? InvisStatus.Fading : (InvisStatus?)null;
        }
    }
}
