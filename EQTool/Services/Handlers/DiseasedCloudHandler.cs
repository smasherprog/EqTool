using EQTool.Models;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class DiseasedCloudHandler : BaseHandler
    {
        private CancellationTokenSource _cts;

        public DiseasedCloudHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.SpellCastOnYouEvent += LogEvents_SpellCastOnYouEvent;
            logEvents.SpellCastOnOtherEvent += LogEvents_SpellCastOnOtherEvent;
            logEvents.ResistSpellEvent += LogEvents_ResistSpellEvent;
            logEvents.DragonRoarEvent += LogEvents_DragonRoarEvent; ;
        }

        private void LogEvents_DragonRoarEvent(object sender, DragonRoarEvent e)
        {
            if (e.Spell?.name == "Diseased Cloud")
            {
                OnDiseasedCloudDetected();
            }
        }

        private void LogEvents_SpellCastOnYouEvent(object sender, SpellCastOnYouEvent e)
        {
            if (e.Spell?.name == "Diseased Cloud")
            {
                OnDiseasedCloudDetected();
            }
        }

        private void LogEvents_SpellCastOnOtherEvent(object sender, SpellCastOnOtherEvent e)
        {
            if (e.Spells?.Any(s => s.name == "Diseased Cloud") == true)
            {
                OnDiseasedCloudDetected();
            }
        }

        private void LogEvents_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            if (e.Spell?.name == "Diseased Cloud")
            {
                OnDiseasedCloudDetected();
            }
        }

        private void OnDiseasedCloudDetected()
        {
            if (!string.Equals(activePlayer?.Player?.Zone, "veeshan", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var doAudio = activePlayer?.Player?.DiseasedCloudAudio ?? false;
            var doOverlay = activePlayer?.Player?.DiseasedCloudOverlay ?? false;
            if (!doAudio && !doOverlay)
            {
                return;
            }
            if (doOverlay)
            {
                logEvents.Handle(new TimerBarEvent
                {
                    Name = $"Word Of Resto",
                    TotalSeconds = 8
                });
            }

            _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                Thread.Sleep(8000);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (doAudio)
                {
                    textToSpeach.Say("Resto Now");
                }
                if (doOverlay)
                {
                    logEvents.Handle(new OverlayEvent { Text = "Resto Now", ForeGround = Brushes.Yellow, Reset = false });
                    Thread.Sleep(3000);
                    if (!token.IsCancellationRequested)
                    {
                        logEvents.Handle(new OverlayEvent { Text = "Resto Now", ForeGround = Brushes.Yellow, Reset = true });
                    }
                }
            });
        }
    }
}
