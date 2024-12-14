using EQTool.Models;
using EQTool.ViewModels;
using System.Collections.Generic;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class SpellWornOffOtherHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        // this list is to let us deal with the fact that the game gives a generic message on charm break, but we'd like to know what the
        // real corresponding spell on the timer window might be, so we can remove it from the timer window
        private readonly List<string> AllCharmSpells = new List<string>()
        {
            "Dictate",
            "Charm",
            "Beguile",
            "Cajoling Whispers",
            "Allure",
            "Boltran's Agacerie",
            "Befriend Animal",
            "Charm Animals",
            "Beguile Plants",
            "Beguile Animals",
            "Allure of the Wild",
            "Call of Karana",
            "Tunare's Request",
            "Dominate Undead",
            "Beguile Undead",
            "Cajole Undead",
            "Thrall of Bones",
            "Enslave Death"
        };

        public SpellWornOffOtherHandler(
            SpellWindowViewModel spellWindowViewModel,
            LogEvents logEvents,
            ActivePlayer activePlayer,
            EQToolSettings eQToolSettings,
            ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            this.logEvents.SpellWornOffOtherEvent += LogEvents_SpellWornOffOtherEvent;
        }

        private void LogEvents_SpellWornOffOtherEvent(object sender, SpellWornOffOtherEvent e)
        {
            // handle the case where the user has asked for audible/visual alerts on any spell fading
            var fadedText = $"{e.SpellName} faded";
            if (activePlayer?.Player?.WornOffAudio == true)
            {
                textToSpeach.Say(fadedText);
            }
            if (activePlayer?.Player?.WornOffOverlay == true)
            {
                _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    logEvents.Handle(new OverlayEvent { Text = fadedText, ForeGround = Brushes.Red, Reset = false });
                    System.Threading.Thread.Sleep(5000);
                    logEvents.Handle(new OverlayEvent { Text = fadedText, ForeGround = Brushes.Red, Reset = true });
                });
            }

            // try and remove the matching spell from the timer list
            spellWindowViewModel.TryRemoveUnambiguousSpellOther(e.SpellName);

            // handle the case where we get the generic Charm Break message, but we'd like to remove the corresponding charm spell from the timer list
            if (e.Line == "Your charm spell has worn off.")
            {
                spellWindowViewModel.TryRemoveUnambiguousSpellOther(AllCharmSpells);
                spellWindowViewModel.TryRemoveUnambiguousSpellSelf(AllCharmSpells);

                // if the user has requested just to be notified for charm breaks
                if (activePlayer?.Player?.CharmBreakAudio == true)
                {
                    textToSpeach.Say($"Charm Break");
                }

                // if the user has requested just to be notified for charm breaks
                var doAlert = activePlayer?.Player?.CharmBreakOverlay ?? false;
                if (doAlert)
                {
                    _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        var text = "Charm Break";
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = false });
                        System.Threading.Thread.Sleep(3000);
                        logEvents.Handle(new OverlayEvent { Text = text, ForeGround = Brushes.Red, Reset = true });
                    });
                }
            }
        }
    }
}
