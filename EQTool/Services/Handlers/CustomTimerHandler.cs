using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;


namespace EQTool.Services.Handlers
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
    public class CustomTimerHandler : BaseHandler
    {
        // set up a regex to watch for formats like
        //      PigTimer-30                  30 second timer, with no name
        //      PigTimer-120-description     120 second timer, with name 'description'
        //      PigTimer-6:40-Guard          6 minutes 40 second timer, with name 'Guard'
        //      PigTimer-10:00               10 minute timer, with no name
        //      PigTimer-1:02:00-LongTimer   1 hour, 2 minute timer, with description 'LongTimer'

        private const string customTimerPattern =
            @"^PigTimer-(((?<hh>[0-9]+):)?((?<mm>[0-9]+):))?(?<ss>[0-9]+)(-(?<label>.+))*";
        //    ^PigTimer-                                                                         must start with PigTimer-
        //               ((?<hh>[0-9]+):)?                                                       Group "hh"      0 or more (sets of numbers, followed by a colon)
        //                                ((?<mm>[0-9]+):)                                       Group "mm"      1 set of numbers followed by a colon
        //              (                                 )?                                                     0 or more hh and mm groups
        //                                                  (?<ss>[0-9]+)                        Group "ss"      1 set of numbers
        //                                                               (-(?<label>\.+))*       Group "label"   0 or more (dash, followed by a set of word characters)
        //
        // https://regex101.com/r/3d1UGb/1
        //
        private readonly Regex regex = new Regex(customTimerPattern, RegexOptions.Compiled);
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        //
        // ctor
        //
        public CustomTimerHandler(
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            EQSpells spells,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.CommsEvent += LogEvents_CommsEvent;
            this.spellWindowViewModel = spellWindowViewModel;
            this.spells = spells;
        }

        //
        // function that gets called whenever a CommsEvent is received
        //
        private void LogEvents_CommsEvent(object sender, CommsEvent commsEvent)
        {
            // use the regex to check for desired content
            var match = regex.Match(commsEvent.Content);
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
                Console.WriteLine($"match found [{match}], hh = [{hh}], mm = [{mm}], ss = [{ss}], label = [{label}], totalseconds = [{timerSeconds}]");

                var spellname = "Feign Death";
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = label != "" ? label : $"{match}",
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds(timerSeconds),
                    TotalRemainingDuration = TimeSpan.FromSeconds(timerSeconds),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.DarkSeaGreen
                });
            }
        }
    }
}
