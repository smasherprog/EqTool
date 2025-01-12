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

        // ctor
        public PetHandler(BaseHandlerData baseHandlerData, PlayerPet playerPet) : base(baseHandlerData)
        {
            this.playerPet = playerPet;

            logEvents.YouBeginCastingEvent += LogEvents_YouBeginCastingEvent;
            logEvents.YouZonedEvent += LogEvents_YouZonedEvent;
        }

        private void LogEvents_YouBeginCastingEvent(object sender, YouBeginCastingEvent e)
        {
            // is this a pet spell ?
            if (playerPet.Pets.PetSpellDictionary.ContainsKey(e.Spell.name))
            {
                // we are casting a pet spell, initialize the pet display
                PetSpell petSpell = playerPet.Pets.PetSpellDictionary[e.Spell.name];
                playerPet.PetSpell = petSpell;

                // testing
                //PetSpell petSpell = playerPet.Pets.PetSpellDictionary["Emissary of Thule"];
                //playerPet.PetSpell = petSpell;
                //playerPet.PetName = "Bakalakadaka";
                //playerPet.CheckMaxMelee(55);
                //playerPet.CheckMaxMelee(61);
            }
        }

        // zoning = loss of pet
        private void LogEvents_YouZonedEvent(object sender, YouZonedEvent e)
        {
            playerPet.Reset();
        }

    }
}
