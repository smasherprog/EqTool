using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    // parser for things specific to _Pets
    public class PetParser : IEqLogParser
    {
        private readonly LogEvents logEvents;

        private const string patternPetNotThere = "You don't have a pet to command!";
        private const string patternPetCreation = @"^(?<pet_name>[\w` ]+) says 'At your service Master.'";
        private const string patternPetReclaimed = @"^(?<pet_name>[\w` ]+) disperses.";
        private const string patternPetLeader = @"^(?<pet_name>[\w` ]+) says 'My leader is (?<leader_name>[\w` ]+).'";
        private const string patternPetDeath = @"^(?<pet_name>[\w` ]+) says 'Sorry to have failed you, oh Great One.'";
        private const string patternPetGetLost = @"^(?<pet_name>[\w` ]+) says 'As you wish, oh great one.'";
        private const string patternPetFollowMe = @"^(?<pet_name>[\w` ]+) says 'Following you, Master.'";
        private const string patternPetSitStand = @"^(?<pet_name>[\w` ]+) says 'Changing position, Master.'";
        private const string patternGuarding = @"^(?<pet_name>[\w` ]+) says 'Guarding with my life..oh splendid one.'";
        private const string patternPetLifetapproc = @"^(?<pet_name>[\w` ]+) beams a smile at [\w` ]+";
        private const string patternPetAttacking = @"^(?<pet_name>[\w` ]+) tells you, 'Attacking (.*?) Master.'";

        private readonly Regex[] regexPatches = new Regex[(int)PetEvent.PetIncident.ANY];
        private readonly List<PetEvent.PetIncident> PetEventTypes = Enum.GetValues(typeof(PetEvent.PetIncident)).Cast<PetEvent.PetIncident>().Where(a => a != PetEvent.PetIncident.NONE && a != PetEvent.PetIncident.ANY).ToList();

        // ctor
        public PetParser(LogEvents logEvents)
        {
            this.logEvents = logEvents;
            regexPatches[(int)PetEvent.PetIncident.CREATION] = new Regex(patternPetCreation, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.RECLAIMED] = new Regex(patternPetReclaimed, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.LEADER] = new Regex(patternPetLeader, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.DEATH] = new Regex(patternPetDeath, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.GETLOST] = new Regex(patternPetGetLost, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.PETLIFETAP] = new Regex(patternPetLifetapproc, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.PETATTACK] = new Regex(patternPetAttacking, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.PETFOLLOWME] = new Regex(patternPetFollowMe, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.SITSTAND] = new Regex(patternPetSitStand, RegexOptions.Compiled);
            regexPatches[(int)PetEvent.PetIncident.GUARD] = new Regex(patternGuarding, RegexOptions.Compiled);
        }


        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var rv = false;

            var petEvent = Match(line, timestamp, lineCounter);
            if (petEvent != null)
            {
                logEvents.Handle(petEvent);
                rv = true;
            }
            return rv;
        }

        private PetEvent Match(string line, DateTime timestamp, int lineCounter, PetEvent.PetIncident petIncident)
        {
            var match = regexPatches[(int)petIncident].Match(line);
            if (match.Success)
            {
                return new PetEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    PetName = match.Groups["pet_name"].Value,
                    Incident = petIncident
                };
            }

            return null;
        }

        // parse this line to see if it contains pet-specific items
        // returns a PetEvent object or null
        public PetEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            // no pet
            if (patternPetNotThere == line)
            {
                return new PetEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    PetName = "None",
                    Incident = PetEvent.PetIncident.NONE
                };
            }

            foreach (var item in PetEventTypes)
            {
                var match = Match(line, timestamp, lineCounter, item);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
