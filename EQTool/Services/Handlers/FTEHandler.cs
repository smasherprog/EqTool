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
        private readonly IAppDispatcher appDispatcher;
        private readonly List<string> NintySevenPercentMobs = new List<string>()
        {
            "Zlandicar",
            "Dozekar the Cursed",
            "Lord Yelinak"
        };

        public FTEHandler(IAppDispatcher appDispatcher, SpellWindowViewModel spellWindowViewModel, LogEvents logEvents, PigParseApi pigParseApi, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach, EQSpells spells) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.appDispatcher = appDispatcher;
            this.pigParseApi = pigParseApi;
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.FTEEvent += LogParser_FTEEvent;
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
                appDispatcher.DispatchUI(() =>
                {
                    spellWindowViewModel.TryAdd(new TimerViewModel
                    {
                        PercentLeft = 100,
                        GroupName = CustomTimer.CustomerTime,
                        Name = $"{start} {e.NPCName}",
                        Rect = spell.Rect,
                        Icon = spell.SpellIcon,
                        TotalDuration = TimeSpan.FromSeconds(61),
                        TotalRemainingDuration = TimeSpan.FromSeconds(61),
                        UpdatedDateTime = DateTime.Now,
                        ProgressBarColor = Brushes.Orchid
                    });
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
                if (doAlert)
                {
                    _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(10000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 50 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("50 seconds");
                        System.Threading.Thread.Sleep(10000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 40 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("40 seconds");
                        System.Threading.Thread.Sleep(10000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 30 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("30 seconds");
                        System.Threading.Thread.Sleep(10000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 20 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("20 seconds");
                        System.Threading.Thread.Sleep(10000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 10 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("10 seconds");
                        System.Threading.Thread.Sleep(5000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 5 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("5 seconds");
                        System.Threading.Thread.Sleep(1000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 4 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("4");
                        System.Threading.Thread.Sleep(1000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 3 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("3");
                        System.Threading.Thread.Sleep(1000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 2 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("2");
                        System.Threading.Thread.Sleep(1000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} 1 seconds", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("1");
                        System.Threading.Thread.Sleep(1000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} Time up", ForeGround = Brushes.Yellow, Reset = false });
                        textToSpeach.Say("Time up");
                        System.Threading.Thread.Sleep(1000);
                        logEvents.Handle(new OverlayEvent { Text = $"{start} Time up", ForeGround = Brushes.Yellow, Reset = true });
                    });
                }
            }
        }
    }
}
