using EQTool.Models;
using EQTool.ViewModels.MobInfoComponents;

namespace EQTool.Services.Handlers
{
    public class PetHandler : BaseHandler
    {
        private readonly PetViewModel playerPet;
        private readonly Pets pets;
        private string lastZoneName = string.Empty;

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
            if (pets.PetSpellDictionary.ContainsKey(e.Spell.name))
            {
                var _PetSpell = pets.PetSpellDictionary[e.Spell.name];
                playerPet.PetSpell = _PetSpell;
            }
        }

        private void LogEvents_LoadingPleaseWaitEvent(object sender, LoadingPleaseWaitEvent e)
        {
            playerPet.Reset();
        }

        private void LogEvents_WelcomeEvent(object sender, WelcomeEvent e)
        {
            playerPet.Reset();
        }

        private void LogEvents_SlainEvent(object sender, SlainEvent e)
        {
            if (e.Victim == "You")
            {
                playerPet.Reset();
            }
        }

        private void LogEvents_SpellWornOffOtherEvent(object sender, SpellWornOffOtherEvent e)
        {
            if (e.Line == "Your charm spell has worn off.")
            {
                playerPet.Reset();
            }
        }

        private void LogEvents_PetEvent(object sender, PetEvent e)
        {

            if (e.Incident == PetEvent.PetIncident.NONE)
            {
                playerPet.Reset();
            }

            else if (e.Incident == PetEvent.PetIncident.CREATION)
            {
                // an unknown pet name screens out almost all other players' pet creation messages
                if (playerPet.IsPetNameKnown == false)
                {
                    playerPet.PetName = e.PetName;
                }
            }

            else if (e.Incident == PetEvent.PetIncident.RECLAIMED)
            {
                if (e.PetName == playerPet.PetName)
                {
                    playerPet.Reset();
                }
            }

            // other pet commands (follow, guard, sit) are visible from every nearby pet, so they
            // cannot distinguish ours from someone else's
            else if (e.Incident == PetEvent.PetIncident.LEADER
                || e.Incident == PetEvent.PetIncident.PETATTACK)
            {
                playerPet.PetName = e.PetName;
            }

            else if (e.Incident == PetEvent.PetIncident.DEATH)
            {
                if (e.PetName == playerPet.PetName)
                {
                    playerPet.Reset();
                }
            }

            else if (e.Incident == PetEvent.PetIncident.GETLOST)
            {
                if (e.PetName == playerPet.PetName)
                {
                    playerPet.Reset();
                }
            }
        }

        private void LogEvents_DamageEvent(object sender, DamageEvent e)
        {
            if (playerPet.IsPetNameKnown)
            {
                if (playerPet.PetName == e.AttackerName)
                {
                    // backstab and kick are excluded: their damage does not track pet rank
                    if ((e.DamageType != "backstabs") && (e.DamageType != "kicks"))
                    {
                        // check the max damage / get pet rank
                        playerPet.CheckMaxMelee(e.DamageDone);
                    }
                }
            }
        }
    }
}
