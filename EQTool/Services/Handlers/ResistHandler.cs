using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class ResistHandler : BaseHandler
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly FightHistory fightHistory;
        private readonly SpellHandlerService spellHandlerService;
        private readonly EQSpells eQSpells;

        public ResistHandler(SpellWindowViewModel spellWindowViewModel, FightHistory fightHistory, BaseHandlerData baseHandlerData, SpellHandlerService spellHandlerService, EQSpells eQSpells) : base(baseHandlerData)
        {
            this.spellWindowViewModel = spellWindowViewModel;
            logEvents.ResistSpellEvent += LogParser_ResistSpellEvent;
            this.fightHistory = fightHistory;
            this.spellHandlerService = spellHandlerService;
            this.eQSpells = eQSpells;
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            var doAlert = activePlayer?.Player?.ResistWarningAudio ?? false;
            var target = e.isYou ? "You " : "Your target ";
            var text = $"{target} resisted the {e.Spell.name} spell";
            if (doAlert)
            {
                // keep audible bandwidth low, let more detail show up in written alert
                textToSpeach.Say($"{target} resisted");
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

            if (SpellHandlerService.SpellsThatNeedCounts.Any(a => a == e.Spell.name))
            {
                appDispatcher.DispatchUI(() =>
                {
                    var currentarget = fightHistory.GetMostRecentTarget(e.TimeStamp);
                    CounterViewModel spell = null;
                    if (!string.IsNullOrWhiteSpace(currentarget))
                    {
                        spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Target.Contains(currentarget) && a.Id == e.Spell.name && a.SpellViewModelType == SpellViewModelType.Counter) as CounterViewModel;
                        if (spell == null)
                        {
                            eQSpells.AllSpells.TryGetValue(e.Spell.name, out var s);
                            spellHandlerService.Handle(s, string.Empty, currentarget, 0, e.TimeStamp);  //TODO: Determine caster?
                        }
                        else
                        {
                            spell.AddCount(!e.isYou ? EQSpells.SpaceYou : null);
                        }
                    }
                    else
                    {
                        spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Id == e.Spell.name && a.SpellViewModelType == SpellViewModelType.Counter) as CounterViewModel;
                        if (spell != null)
                        {
                            spell.AddCount(!e.isYou ? EQSpells.SpaceYou : null);
                        }
                    }
                });
            }
        }
    }
}
