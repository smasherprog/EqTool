﻿using EQTool.Models;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using System.Linq;

namespace EQTool.Services
{
    public class SavePlayerStateService
    {
        private readonly EQToolSettingsLoad toolSettingsLoad;
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQToolSettings eQToolSettings;

        public SavePlayerStateService(EQToolSettingsLoad toolSettingsLoad,
            ActivePlayer activePlayer,
            IAppDispatcher appDispatcher,
            EQToolSettings eQToolSettings,
            SpellWindowViewModel spellWindowViewModel)
        {
            this.toolSettingsLoad = toolSettingsLoad;
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
            this.spellWindowViewModel = spellWindowViewModel;
            this.eQToolSettings = eQToolSettings;
        }
        
        public void TrySaveYouSpellData()
        {
            if (activePlayer?.Player != null)
            {
                appDispatcher.DispatchUI(() =>
                {
                    if (!activePlayer.Player.YouSpells.Any())
                    {
                        var before = activePlayer.Player.YouSpells ?? new System.Collections.Generic.List<YouSpells>();
                        activePlayer.Player.YouSpells = spellWindowViewModel.SpellList.Where(a => a.GroupName == EQSpells.SpaceYou && a.SpellViewModelType == SpellViewModelType.Spell)
                            .Cast<SpellViewModel>()
                            .Select(a => new YouSpells
                            {
                                Name = a.Name,
                                Caster = a.Caster,
                                TotalSecondsLeft = (int)a.TotalRemainingDuration.TotalSeconds,
                            }).ToList();
                        toolSettingsLoad.Save(eQToolSettings);
                    } 
                });
            }
        }
    }
}
