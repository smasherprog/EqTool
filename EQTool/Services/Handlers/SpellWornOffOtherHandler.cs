using EQTool.Models;
using EQTool.ViewModels;
using System.Collections.Generic;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class SpellWornOffOtherHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
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
            spellWindowViewModel.TryRemoveUnambiguousSpellOther(e.SpellName);
            if (e.Line == "Your charm spell has worn off.")
            {
                spellWindowViewModel.TryRemoveUnambiguousSpellOther(AllCharmSpells);
                spellWindowViewModel.TryRemoveUnambiguousSpellSelf(AllCharmSpells);
                if (activePlayer?.Player?.CharmBreakAudio == true)
                {
                    textToSpeach.Say($"Charm Break");
                }

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
