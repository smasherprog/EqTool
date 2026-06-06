using EQTool.Models;
using System;
using System.Linq;
using System.Threading;

namespace EQTool.Services.Handlers
{
    public class ZlandicarHandler : BaseHandler
    {
        private CancellationTokenSource _cts;
        private readonly string[] spellsname = new[] { "Stun Breath", "Dragon Roar" };

        public ZlandicarHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            logEvents.SpellCastOnYouEvent += LogEvents_SpellCastOnYouEvent;
            logEvents.SpellCastOnOtherEvent += LogEvents_SpellCastOnOtherEvent;
            logEvents.ResistSpellEvent += LogEvents_ResistSpellEvent;
            logEvents.DragonRoarEvent += LogEvents_DragonRoarEvent; ;
        }

        private void LogEvents_DragonRoarEvent(object sender, DragonRoarEvent e)
        {
            if (spellsname.Contains(e.Spell?.name))
            {
                Handle(e.Spell);
            }
        }

        private void LogEvents_SpellCastOnYouEvent(object sender, SpellCastOnYouEvent e)
        {
            if (spellsname.Contains(e.Spell?.name))
            {
                Handle(e.Spell);
            }
        }

        private void LogEvents_SpellCastOnOtherEvent(object sender, SpellCastOnOtherEvent e)
        {
            if (e.Spells?.Any(s => spellsname.Contains(s.name)) == true)
            {
                Handle(e.Spells.FirstOrDefault(s => spellsname.Contains(s.name)));
            }
        }

        private void LogEvents_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            if (spellsname.Contains(e.Spell?.name))
            {
                Handle(e.Spell);
            }
        }

        private void Handle(Spell spell)
        {
            if (!string.Equals(activePlayer?.Player?.Zone, "necropolis", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = _cts.Token;

            var doAudio = activePlayer?.Player?.ZlandicarAudio ?? false;
            var doOverlay = activePlayer?.Player?.ZlandicarOverlay ?? false;
            if (!doAudio && !doOverlay)
            {
                return;
            }
            if (doOverlay)
            {
                if (spell.name == "Dragon Roar")
                {
                    logEvents.Handle(new TimerBarEvent
                    {
                        Name = $"Dragon Roar",
                        TotalSeconds = 36
                    });
                }
                else if (spell.name == "Stun Breath")
                {
                    logEvents.Handle(new TimerBarEvent
                    {
                        Name = $"Stun Breath",
                        TotalSeconds = 12
                    });
                }
            }

            //_ = System.Threading.Tasks.Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(7000);
            //    if (token.IsCancellationRequested)
            //    {
            //        return;
            //    }

            //    if (doAudio)
            //    {
            //        textToSpeach.Say("Resto Now");
            //    }
            //    if (doOverlay)
            //    {
            //        logEvents.Handle(new OverlayEvent { Text = "Resto Now", ForeGround = Brushes.Yellow, Reset = false });
            //        Thread.Sleep(3000);
            //        if (!token.IsCancellationRequested)
            //        {
            //            logEvents.Handle(new OverlayEvent { Text = "Resto Now", ForeGround = Brushes.Yellow, Reset = true });
            //        }
            //    }
            //});
        }
    }
}
