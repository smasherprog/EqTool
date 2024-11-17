using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class ResistSpellHandler : BaseHandler
    {
        private readonly List<string> SpellsThatDragonsDo = new List<string>()
        {
            "Dragon Roar",
            "Silver Breath",
            "Ice breath",
            "Mind Cloud",
            "Rotting Flesh",
            "Putrefy Flesh",
            "Stun Breath",
            "Immolating Breath",
            "Rain of Molten Lava",
            "Frost Breath",
            "Lava Breath",
            "Cloud of Fear",
            "Diseased Cloud",
            "Tsunami",
            "Ancient Breath"
        };
        private readonly SpellWindowViewModel spellWindowViewModel;

        public ResistSpellHandler(
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.ResistSpellEvent += LogParser_ResistSpellEvent;
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            var doAlert = activePlayer?.Player?.ResistWarningAudio ?? false;
            var target = e.isYou ? "You " : "Your target ";
            var text = $"{target} resisted the {e.Spell.name} spell";
            if (doAlert)
            {
                textToSpeach.Say(text);
            }
            doAlert = activePlayer?.Player?.ResistWarningOverlay ?? false;
            if (doAlert)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(3000);
                    logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                });
            }

            if (SpellsThatDragonsDo.Contains(e.Spell.name))
            {
                spellWindowViewModel.TryAdd(new TimerViewModel
                {
                    PercentLeft = 100,
                    GroupName = CustomTimer.CustomerTime,
                    Name = e.Spell.name,
                    Rect = e.Spell.Rect,
                    Icon = e.Spell.SpellIcon,
                    TotalDuration = TimeSpan.FromSeconds((int)(e.Spell.recastTime / 1000.0)),
                    TotalRemainingDuration = TimeSpan.FromSeconds((int)(e.Spell.recastTime / 1000.0)),
                    UpdatedDateTime = DateTime.Now
                });
            }
        }
    }
}
