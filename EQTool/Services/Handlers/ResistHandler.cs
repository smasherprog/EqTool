using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System.Linq;

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
            // The resist overlay/TTS alert now lives as the "Resist" built-in trigger (see
            // BuiltInTriggers.CreateResist). This handler only keeps the resist counter tracking
            // used for spells that need hit counts.
            if (SpellHandlerService.SpellsThatNeedCounts.Any(a => a == e.Spell.name))
            {
                appDispatcher.DispatchUI(() =>
                {
                    var currentarget = fightHistory.GetMostRecentTarget(e.TimeStamp);
                    CounterViewModel spell = null;
                    if (!string.IsNullOrWhiteSpace(currentarget))
                    {
                        spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.GroupName.Contains(currentarget) && a.Name == e.Spell.name && a.SpellViewModelType == ViewModels.SpellWindow.SpellViewModelType.Counter) as CounterViewModel;
                        if (spell == null)
                        {
                            eQSpells.AllSpells.TryGetValue(e.Spell.name, out var s);
                            spellHandlerService.Handle(s, currentarget, 0, e.TimeStamp);
                        }
                        else
                        {
                            spell.Count++;
                        }
                    }
                    else
                    {
                        spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == e.Spell.name && a.SpellViewModelType == ViewModels.SpellWindow.SpellViewModelType.Counter) as CounterViewModel;
                        if (spell != null)
                        {
                            spell.Count++;
                        }
                    }
                });
            }
        }
    }
}
