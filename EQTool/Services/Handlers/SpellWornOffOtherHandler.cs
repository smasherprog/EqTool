using EQTool.Models;
using EQTool.ViewModels;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class SpellWornOffOtherHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;

        public SpellWornOffOtherHandler(SpellWindowViewModel spellWindowViewModel, BaseHandlerData baseHandlerData) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.SpellWornOffOtherEvent += LogEvents_SpellWornOffOtherEvent;
        }

        private void LogEvents_SpellWornOffOtherEvent(object sender, SpellWornOffOtherEvent e)
        {
            if (EQSpells.SpellsThatNeedCounts.Contains(e.SpellName))
            {
                return;
            }

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
                // go find the charm spell on the timer bar and remove it
                spellWindowViewModel.TryRemoveUnambiguousSpellOther(EQSpells.Charms);
                spellWindowViewModel.TryRemoveUnambiguousSpellSelf(EQSpells.Charms);

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
