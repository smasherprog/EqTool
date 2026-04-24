using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class FTEHandler : BaseHandler
    {
        private readonly PigParseApi pigParseApi;
        private readonly EQSpells spells;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly List<string> NintySevenPercentMobs = new List<string>()
        {
            "Zlandicar",
            "Dozekar the Cursed",
            "Lord Yelinak"
        };
        private readonly List<string> NintySixPercentMobs = new List<string>()
        {
            "Dozekar the Cursed",
            "Lord Yelinak"
        };

        public FTEHandler(EQSpells spells, SpellWindowViewModel spellWindowViewModel, PigParseApi pigParseApi, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.pigParseApi = pigParseApi;
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.FTEEvent += LogParser_FTEEvent;
        }

        private void LogParser_FTEEvent(object sender, FTEEvent e)
        {
            if (activePlayer?.Player?.FTEAudio == true)
            {
                textToSpeach.Say($"{e.FTEPerson} F T E {e.NPCName}");
            }
            var doAlert = activePlayer?.Player?.FTEOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    var fteperson = pigParseApi.GetPlayerData(e.FTEPerson, activePlayer.Player.Server.Value);
                    var text = $"{e.FTEPerson} FTE {e.NPCName}";
                    if (fteperson != null)
                    {
                        text = $"{fteperson.Name} <{fteperson.GuildName}> FTE {e.NPCName}";
                    }

                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Yellow, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Yellow, Reset = true });
                });
            }
            var start = "--97% Rule--";
            if (NintySevenPercentMobs.Contains(e.NPCName))
            {
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == "Spirit of Wolf");
                var timeleft = 61;
                if (NintySixPercentMobs.Contains(e.NPCName) && activePlayer.Player.Server == EQToolShared.Enums.Servers.Green)
                {
                    start = "--96% Rule--";
                    timeleft = 91;
                }

                appDispatcher.DispatchUI(() =>
                {
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        GroupName = CustomTimer.CustomerTime,
                        Name = $"{start} {e.NPCName}",
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        TotalDuration = TimeSpan.FromSeconds(timeleft),
                        TotalRemainingDuration = TimeSpan.FromSeconds(timeleft),
                        UpdatedDateTime = DateTime.Now,
                        ProgressBarColor = Brushes.Orchid
                    }, true);
                });
            }
            if (e.NPCName == "Lodizal")
            {
                var spell = spells.AllSpells.FirstOrDefault(a => a.name == "Spirit of Wolf");
                appDispatcher.DispatchUI(() =>
                {
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        GroupName = CustomTimer.CustomerTime,
                        Name = $"--5 Minute Rule-- {e.NPCName}",
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        TotalDuration = TimeSpan.FromMinutes(5),
                        TotalRemainingDuration = TimeSpan.FromMinutes(5),
                        UpdatedDateTime = DateTime.Now,
                        ProgressBarColor = Brushes.Orchid
                    });
                });
            }
            if (NintySevenPercentMobs.Contains(e.NPCName))
            {
                doAlert = activePlayer?.Player?.FTETimerOverlay ?? false;

            }
        }
    }
}
