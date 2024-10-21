using EQTool.Models;
using EQToolShared.HubModels;
using System;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    //
    // Class to parse for a custom timer, in the form of one of the following:
    //      .ct-duration
    //      PigTimer-duration-label
    // where
    //      PigTimer (short of Custom Timer) is the identifying marker
    //      duration can be in formats:
    //          hh:mm:ss
    //          mm:ss
    //          ss
    //      content can be in a tell, or in a visible channel, like /say
    //      Note: there cannot be any blank spaces
    //
    // Examples:
    //      PigTimer-30                      30 second timer, with no name
    //      PigTimer-10                      10 second timer, with no name
    //      PigTimer-10:00                   10 minute timer, with no name
    //      PigTimer-10:00:00                10 hour timer, with no name
    //      PigTimer-120-description         120 second timer, with name 'description'
    //      PigTimer-6:40-Tim_the_Mighty     6 minutes 40 second timer, with name 'Tim_the_Mighty'
    //      PigTimer-1:02:00-LongTimer       1 hour, 2 minute timer, with description 'LongTimer'
    //
    public class LogStartCustomTimer : IEqLogParseHandler
    {
        private readonly LogEvents logEvents;

        //
        // ctor
        //
        public LogStartCustomTimer(LogEvents logEvents)
        {
            this.logEvents = logEvents;
        }

        //
        // overload the Handle method to perform the specific parsing
        //
        public bool Handle(string line, DateTime timestamp)
        {
            var m = ParseStartTimer(line);
            if (m != null)
            {
                logEvents.Handle(new StartTimerEvent { CustomTimer = m, TimeStamp = timestamp });
                return true;
            }
            return false;
        }

        //
        // parse the passed line to see if user has requested a custom timer to started
        //
        public CustomTimer ParseStartTimer(string line)
        {
            // return value
            CustomTimer rv = null;

            // set up a crazy regex to watch for formats like
            //      PigTimer-30                  30 second timer, with no name
            //      PigTimer-120-description     120 second timer, with name 'description'
            //      PigTimer-6:40-Guard          6 minutes 40 second timer, with name 'Guard'
            //      PigTimer-10:00               10 minute timer, with no name
            //      PigTimer-1:02:00-LongTimer   1 hour, 2 minute timer, with description 'LongTimer'
            var customTimerPattern = @"PigTimer-(((?<hh>[0-9]+):)?((?<mm>[0-9]+):))?(?<ss>[0-9]+)(-(?<label>\w+))*";
            //                            PigTimer-                                                                         must start with PigTimer-
            //                                      ((?<hh>[0-9]+):)?                                                       Group "hh"      0 or more (sets of numbers, followed by a colon)
            //                                                       ((?<mm>[0-9]+):)                                       Group "mm"      1 set of numbers followed by a colon
            //                                     (                                 )?                                                     0 or more hh and mm groups
            //                                                                         (?<ss>[0-9]+)                        Group "ss"      1 set of numbers
            //                                                                                      (-(?<label>\w+))*       Group "label"   0 or more (dash, followed by a set of word characters)
            //
            // This website is absolutely needed when creating this kind of crazy regex!
            //      https://regex101.com/r/RWA9Oy/1
            var regex = new Regex(customTimerPattern, RegexOptions.Compiled);
            var match = regex.Match(line);
            if (match.Success)
            {
                // get results from the rexex scan
                var hh = match.Groups["hh"].Value;
                var mm = match.Groups["mm"].Value;
                var ss = match.Groups["ss"].Value;
                var label = match.Groups["label"].Value;

                // count up the seconds
                var timerSeconds = 0;
                if (ss != "")
                {
                    timerSeconds += int.Parse(ss);
                }
                if (mm != "")
                {
                    timerSeconds += 60 * int.Parse(mm);
                }
                if (hh != "")
                {
                    timerSeconds += 3600 * int.Parse(hh);
                }
                Console.WriteLine($"match found [{match}], [{hh}], [{mm}], [{ss}], [{label}], [{timerSeconds}]");

                // return value
                rv = new CustomTimer
                {
                    DurationInSeconds = timerSeconds
                };
                // if the user didn't specify a label, we'll give it the match string
                rv.Name = label != "" ? label : $"{match}";
            }
            return rv;
        }
    }
}
