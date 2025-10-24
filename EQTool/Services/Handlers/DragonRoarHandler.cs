using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EQTool.Services.Handlers
{
    public class DragonRoarHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;

        public DragonRoarHandler(EQSpells spells, SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.DragonRoarEvent += LogEvents_DragonRoarEvent;
            logEvents.DragonRoarRemoteEvent += LogEvents_DragonRoarRemoteEvent;
        }

        private void LogEvents_DragonRoarRemoteEvent(object sender, DragonRoarRemoteEvent e)
        {
            var ploc = activePlayer.Location;
            if (ploc.HasValue && e.Location.HasValue)
            {
                if (Point3D.Subtract(e.Location.Value, ploc.Value).Length > 1000)
                {
                    return;
                }
            }
            LogEvents_DragonRoarEvent(spells.AllSpells.FirstOrDefault(a => a.name == e.SpellName));
        }

        private void LogEvents_DragonRoarEvent(object sender, DragonRoarEvent e)
        {
            LogEvents_DragonRoarEvent(e.Spell);
        }

        private void LogEvents_DragonRoarEvent(Spell spell)
        {
            appDispatcher.DispatchUI(() =>
            {
                if (spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Target == CustomTimer.CustomerTime && a.Id == spell.name) is TimerViewModel exists && exists.TotalRemainingDuration.TotalSeconds > 2)
                {
                    return;
                }

                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    Target = CustomTimer.CustomerTime,
                    Id = spell.name,
                    Rect = spell.Rect,
                    Icon = spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds((int)(spell.recastTime / 1000.0)),
                    TotalRemainingDuration = TimeSpan.FromSeconds((int)(spell.recastTime / 1000.0)),
                    UpdatedDateTime = DateTime.Now,
                    ProgressBarColor = Brushes.DarkOrange
                });

                if (spell.name == "Dragon Roar")
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
            });

        }
    }
}
