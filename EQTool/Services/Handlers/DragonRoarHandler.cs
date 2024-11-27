using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class DragonRoarHandler : BaseHandler
    { 
        private readonly SpellWindowViewModel spellWindowViewModel;

        public DragonRoarHandler(SpellWindowViewModel spellWindowViewModel, LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.DragonRoarEvent += LogEvents_DragonRoarEvent;
        }

        private void LogEvents_DragonRoarEvent(object sender, DragonRoarEvent e)
        { 
            spellWindowViewModel.TryAdd(new TimerViewModel
            {
                PercentLeft = 100,
                GroupName = CustomTimer.CustomerTime,
                Name = e.Spell.name,
                Rect = e.Spell.Rect,
                Icon = e.Spell.SpellIcon,
                TotalDuration = TimeSpan.FromSeconds((int)(e.Spell.recastTime  / 1000.0)),
                TotalRemainingDuration = TimeSpan.FromSeconds((int)(e.Spell.recastTime / 1000.0)),
                UpdatedDateTime = DateTime.Now,
                ProgressBarColor = Brushes.DarkOrange
            });
             
            if (e.Spell.name == "Dragon Roar")
            {

                var doAlert = activePlayer?.Player?.DragonRoarAudio ?? false;
                if (doAlert)
                {
                    _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        textToSpeach.Say($"Dragon Roar out");
                        System.Threading.Thread.Sleep(1000 * 30);
                        textToSpeach.Say($"Dragon Roar in 6 Seconds!");
                        System.Threading.Thread.Sleep(2000);
                        textToSpeach.Say($"4 Seconds!");
                        System.Threading.Thread.Sleep(2000);
                        textToSpeach.Say($"2");
                        System.Threading.Thread.Sleep(1000);
                        textToSpeach.Say($"1");
                    });
                }

                doAlert = activePlayer?.Player?.DragonRoarOverlay ?? false;
                if (doAlert)
                {
                    _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(1000 * 30);
                        var text = "Dragon Roar in 6 Seconds!";
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                        System.Threading.Thread.Sleep(1000);

                        text = "Dragon Roar in 5 Seconds!";
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                        System.Threading.Thread.Sleep(1000);

                        text = "Dragon Roar in 4 Seconds!";
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                        System.Threading.Thread.Sleep(1000);

                        text = "Dragon Roar in 3 Seconds!";
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                        System.Threading.Thread.Sleep(1000);

                        text = "Dragon Roar in 2 Seconds!";
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                        System.Threading.Thread.Sleep(1000);

                        text = "Dragon Roar in 1 Seconds!";
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                        System.Threading.Thread.Sleep(1000);
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                    });
                }
            }
        }
    }
}
