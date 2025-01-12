using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.Services.Handlers
{
    // handler for events fired impacting pets
    public class PetHandler : BaseHandler
    {
        // reference to DI global
        private readonly PlayerPet playerPet;
        private readonly SettingsWindowViewModel viewModel;

        // ctor
        public PetHandler(BaseHandlerData baseHandlerData, PlayerPet playerPet, SettingsWindowViewModel vm) : base(baseHandlerData)
        //public PetHandler(BaseHandlerData baseHandlerData) : base(baseHandlerData)
        //public PetHandler(BaseHandlerData baseHandlerData, PlayerPet playerPet) : base(baseHandlerData)
        //public PetHandler(BaseHandlerData baseHandlerData, SettingsWindowViewModel vm) : base(baseHandlerData)
        {
            viewModel = vm;
            this.playerPet = playerPet;

            logEvents.YouBeginCastingEvent += LogEvents_YouBeginCastingEvent;
        }

        private void LogEvents_YouBeginCastingEvent(object sender, YouBeginCastingEvent e)
        {
            //PlayerPet pp = viewModel.PetViewModel.playerPet;

            // is this a pet spell ?
            if (playerPet.Pets.PetSpellDictionary.ContainsKey(e.Spell.name))
            {
                // wake up the pet displays
                PetSpell ps = playerPet.Pets.PetSpellDictionary[e.Spell.name];
                viewModel.PetViewModel.SetPetSpell(ps);
                playerPet.PetSpell = ps;

                //Pets pets = playerPet.Pets;
                //PetSpell p = pets.PetSpellDictionary["Emissary of Thule"];
                //PetSpell p = pets.PetSpellDictionary["Minion of Shadows"];
                //PetSpell p = pets.PetSpellDictionary["Leering Corpse"];
                //playerPet.PetSpell = p;

                //playerPet.PetName = "Bakalakadaka";
                //rankIndex = 1;

            }


        }



    }
}
