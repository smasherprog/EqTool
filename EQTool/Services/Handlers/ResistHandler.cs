using System;
using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System.Linq;
using System.Windows.Media;

namespace EQTool.Services.Handlers
{
    public class ResistHandler : BaseHandler
    {
        private readonly CooldownHandlerService cooldownService;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly FightHistory fightHistory;
        private readonly SpellHandlerService spellHandlerService;
        private readonly EQSpells eQSpells;

        public ResistHandler(
            CooldownHandlerService cooldownService,
            SpellHandlerService spellHandlerService,
            SpellWindowViewModel spellWindowViewModel,
            FightHistory fightHistory,
            BaseHandlerData baseHandlerData,
            EQSpells eQSpells) : base(baseHandlerData)
        {
            this.cooldownService = cooldownService;
            this.spellWindowViewModel = spellWindowViewModel;
            this.fightHistory = fightHistory;
            this.spellHandlerService = spellHandlerService;
            this.eQSpells = eQSpells;
            
            logEvents.ResistSpellEvent += LogParser_ResistSpellEvent;
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellEvent e)
        {
            var casterName = !e.isYou ? EQSpells.SpaceYou : null;   // Not a perfect guess. You could technically have cast it on yourself and resisted it.
            var targetName = e.isYou ? EQSpells.SpaceYou : fightHistory.GetMostRecentTarget(e.TimeStamp);
            var isNpc = !e.isYou && targetName != null;
            
            HandleAlert(e);

            if (EQSpells.SpellsThatNeedCounts.Any(a => a == e.Spell.name))
                HandleCountTimer(e, targetName, casterName, isNpc);
            else
                cooldownService.AddCooldownTimerIfNecessary(e.Spell, casterName, targetName, isNpc, 0, e.TimeStamp);
        }

        private void HandleAlert(ResistSpellEvent e)
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
        }

        private void HandleCountTimer(ResistSpellEvent e, string targetName, string casterName, bool isNpc)
        {
            var shouldTryAddCooldown = true;
            appDispatcher.DispatchUI(() =>
            {
                CounterViewModel spell = null;
                if (!string.IsNullOrWhiteSpace(targetName))
                {
                    spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Target.Contains(targetName) && a.Id == e.Spell.name && a.SpellViewModelType == SpellViewModelType.Counter) as CounterViewModel;
                    if (spell == null)
                    {
                        eQSpells.AllSpells.TryGetValue(e.Spell.name, out var s);
                        spellHandlerService.Handle(s, casterName, targetName, 0, e.TimeStamp);
                        shouldTryAddCooldown = false;    // Spell handler also parses cooldowns so no need to do anything for that here. I don't love this but what can you do.
                    }
                    else
                    {
                        spell.AddCount(casterName);
                    }
                }
                else
                {
                    spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Id == e.Spell.name && a.SpellViewModelType == SpellViewModelType.Counter) as CounterViewModel;
                    spell?.AddCount(casterName);
                }
            });
            
            if (shouldTryAddCooldown)
                cooldownService.AddCooldownTimerIfNecessary(e.Spell, casterName, targetName, isNpc, 0, e.TimeStamp);
        }
    }
}
