using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class DragonRoarHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        private readonly IAppDispatcher appDispatcher;

        public DragonRoarHandler(IAppDispatcher appDispatcher, EQSpells spells, SpellWindowViewModel spellWindowViewModel, LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.appDispatcher = appDispatcher;
            this.spells = spells;
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.DragonRoarEvent += LogEvents_DragonRoarEvent;
            this.logEvents.DragonRoarRemoteEvent += LogEvents_DragonRoarRemoteEvent;
        }

        private void LogEvents_DragonRoarRemoteEvent(object sender, DragonRoarRemoteEvent e)
        {
            LogEvents_DragonRoarEvent(spells.AllSpells.FirstOrDefault(a=> a.name == e.SpellName));
        }

        private void LogEvents_DragonRoarEvent(object sender, DragonRoarEvent e)
        {
            LogEvents_DragonRoarEvent(e.Spell);
        }

        private void LogEvents_DragonRoarEvent(Spell spell)
        {
            this.appDispatcher.DispatchUI(() =>
            {
                var exists = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.GroupName == CustomTimer.CustomerTime && a.Name == spell.name) as TimerViewModel;
                if (exists != null && exists.TotalRemainingDuration.TotalSeconds > 2)
                {
                    return;
                }

                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = spell.name,
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
