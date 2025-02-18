using EQTool.Models;
using EQTool.ViewModels.MobInfoComponents;

namespace EQTool.Services.Handlers
{
    // handler for events fired impacting _Pets
    public class PetHandler : BaseHandler
    {
        // reference to DI global
        private readonly PetViewModel playerPet;
        private readonly Pets pets;
        private string lastZoneName = string.Empty;

        // ctor
        public PetHandler(BaseHandlerData baseHandlerData, PetViewModel playerPet, Pets pets) : base(baseHandlerData)
        {
            this.playerPet = playerPet;
            this.pets = pets;
            logEvents.YouBeginCastingEvent += LogEvents_YouBeginCastingEvent;
            logEvents.LoadingPleaseWaitEvent += LogEvents_LoadingPleaseWaitEvent;
            logEvents.WelcomeEvent += LogEvents_WelcomeEvent;
            logEvents.YouZonedEvent += LogEvents_YouZonedEvent;
            logEvents.SlainEvent += LogEvents_SlainEvent;
            logEvents.SpellWornOffOtherEvent += LogEvents_SpellWornOffOtherEvent;
            logEvents.PetEvent += LogEvents_PetEvent;
            logEvents.DamageEvent += LogEvents_DamageEvent;
        }

        private void LogEvents_YouZonedEvent(object sender, YouZonedEvent e)
        {
            if (e.ShortName != lastZoneName)
            {
                lastZoneName = e.ShortName;
                playerPet.Reset();
            }
        }

        private void LogEvents_YouBeginCastingEvent(object sender, YouBeginCastingEvent e)
        {
            // is this a pet spell ?
            if (pets.PetSpellDictionary.ContainsKey(e.Spell.name))
            {
                // we are casting a pet spell, initialize the pet display
                var _PetSpell = pets.PetSpellDictionary[e.Spell.name];
                playerPet.PetSpell = _PetSpell;
            }
        }

        // zoning = loss of pet
        private void LogEvents_LoadingPleaseWaitEvent(object sender, LoadingPleaseWaitEvent e)
        {
            playerPet.Reset();
        }

        // entering game = ensure starting with no pet
        private void LogEvents_WelcomeEvent(object sender, WelcomeEvent e)
        {
            playerPet.Reset();
        }

        // player death = loss of pet
        private void LogEvents_SlainEvent(object sender, SlainEvent e)
        {
            if (e.Victim == "You")
            {
                playerPet.Reset();
            }
        }

        // charm break = loss of pet
        private void LogEvents_SpellWornOffOtherEvent(object sender, SpellWornOffOtherEvent e)
        {
            if (e.Line == "Your charm spell has worn off.")
            {
                playerPet.Reset();
            }
        }

        // pet-specific incidents
        private void LogEvents_PetEvent(object sender, PetEvent e)
        {

            // pet not there
            if (e.Incident == PetEvent.PetIncident.NONE)
            {
                playerPet.Reset();
            }

            // pet created - save the name
            else if (e.Incident == PetEvent.PetIncident.CREATION)
            {
                // do we not yet know the pet name?
                // this should screen out almost all other players pet creation messages
                if (playerPet.IsPetNameKnown == false)
                {
                    playerPet.PetName = e.PetName;
                }
            }

            // pet reclaimed
            else if (e.Incident == PetEvent.PetIncident.RECLAIMED)
            {
                // our pet?
                if (e.PetName == playerPet.PetName)
                {
                    playerPet.Reset();
                }
            }

            // pet
            else if (e.Incident == PetEvent.PetIncident.LEADER
                || e.Incident == PetEvent.PetIncident.PETATTACK
                || e.Incident == PetEvent.PetIncident.PETFOLLOWME
                || e.Incident == PetEvent.PetIncident.SITSTAND
                || e.Incident == PetEvent.PetIncident.GUARD
                || e.Incident == PetEvent.PetIncident.PETLIFETAP)
            {
                playerPet.PetName = e.PetName;
            }

            // pet death
            else if (e.Incident == PetEvent.PetIncident.DEATH)
            {
                // our pet?
                if (e.PetName == playerPet.PetName)
                {
                    playerPet.Reset();
                }
            }

            // pet get lost
            else if (e.Incident == PetEvent.PetIncident.GETLOST)
            {
                // our pet?
                if (e.PetName == playerPet.PetName)
                {
                    playerPet.Reset();
                }
            }
        }

        private void LogEvents_DamageEvent(object sender, DamageEvent e)
        {
            // is pet name known?
            if (playerPet.IsPetNameKnown)
            {
                // damage from our pet?
                if (playerPet.PetName == e.AttackerName)
                {
                    // check the max damage / get pet rank
                    playerPet.CheckMaxMelee(e.DamageDone);
                }
            }
        }
    }
}
