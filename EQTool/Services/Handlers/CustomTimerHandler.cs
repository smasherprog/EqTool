using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;


namespace EQTool.Services.Handlers
{
    public class CustomTimerHandler : BaseHandler
    {
        // Started by a tell or any visible channel, with no blank spaces anywhere:
        //      PigTimer-30                   30 second timer, unnamed
        //      PigTimer-10:00                10 minute timer, unnamed
        //      PigTimer-6:40-Tim_the_Mighty  6m40s timer named 'Tim_the_Mighty'
        //      PigTimer-1:02:00-LongTimer    1h02m timer named 'LongTimer'
        // The hh and mm groups are optional as a pair, so a lone number parses as seconds.
        // https://regex101.com/r/3d1UGb/1
        private const string customTimerPattern =
            @"^PigTimer-(((?<hh>[0-9]+):)?((?<mm>[0-9]+):))?(?<ss>[0-9]+)(-(?<label>.+))*";
        private const string customTimerPatternAlias =
          @"^StartTimer-(((?<hh>[0-9]+):)?((?<mm>[0-9]+):))?(?<ss>[0-9]+)(-(?<label>.+))*";
        private readonly Regex regexAlias = new Regex(customTimerPatternAlias, RegexOptions.Compiled);
        private readonly Regex regex = new Regex(customTimerPattern, RegexOptions.Compiled);
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public CustomTimerHandler(EQSpells spells, SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.CommsEvent += LogEvents_CommsEvent;
            this.spells = spells;
        }

        private void LogEvents_CommsEvent(object sender, CommsEvent commsEvent)
        {
            var match = regex.Match(commsEvent.Content);
            if (!match.Success)
            {
                match = regexAlias.Match(commsEvent.Content);
            }
            if (match.Success)
            {
                var hh = match.Groups["hh"].Value;
                var mm = match.Groups["mm"].Value;
                var ss = match.Groups["ss"].Value;
                var label = match.Groups["label"].Value;

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

                // a custom timer has no icon of its own, so it borrows this spell's artwork
                var spellname = "Feign Death";
                if (spells.AllSpells.TryGetValue(spellname, out var spell))
                {
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
}
