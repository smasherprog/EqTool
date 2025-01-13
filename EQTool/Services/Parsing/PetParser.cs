using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace EQTool.Services.Parsing
{
    // parser for things specific to _Pets
    public class PetParser : IEqLogParser
    {
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;

        private const string patternPetNotThere = "You don't have a pet to command!";
        private const string patternPetCreation = @"^(?<pet_name>[\w` ]+) says 'At your service Master.'";
        private const string patternPetReclaimed = @"^(?<pet_name>[\w` ]+) disperses.";
        private const string patternPetLeader = @"^(?<pet_name>[\w` ]+) says 'My leader is (?<leader_name>[\w` ]+).'";
        private const string patternPetDeath = @"^(?<pet_name>[\w` ]+) says 'Sorry to have failed you, oh Great One.'";
        private const string patternPetGetLost = @"^(?<pet_name>[\w` ]+) says 'As you wish, oh great one.'";

        private readonly Regex regexPetCreation = new Regex(patternPetCreation, RegexOptions.Compiled);
        private readonly Regex regexPetReclaimed = new Regex(patternPetReclaimed, RegexOptions.Compiled);
        private readonly Regex regexPetLeader = new Regex(patternPetLeader, RegexOptions.Compiled);
        private readonly Regex regexPetDeath = new Regex(patternPetDeath, RegexOptions.Compiled);
        private readonly Regex regexPetGetLost = new Regex(patternPetGetLost, RegexOptions.Compiled);

        // ctor
        public PetParser(LogEvents logEvents, ActivePlayer activePlayer)
        {
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
        }


        // handle a line from the log file.
        // If we find what we are seeking, fire off our event
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            bool rv = false;

            PetEvent petEvent = Match(line, timestamp, lineCounter);
            if (petEvent != null)
            {
                logEvents.Handle(petEvent);
                rv = true;
            }
            return rv;
        }

        // parse this line to see if it contains pet-specific items
        // returns a PetEvent object or null
        public PetEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            PetEvent rv = null;

            // no pet
            if (patternPetNotThere == line)
            {
                rv = new PetEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    PetName = "None",
                    Incident = PetEvent.PetIncident.NONE
                };
            }

            // pet creation
            var match = regexPetCreation.Match(line);
            if (match.Success)
            {
                rv = new PetEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    PetName = match.Groups["pet_name"].Value,
                    Incident = PetEvent.PetIncident.CREATION
                };
            }

            // pet reclaimed
            match = regexPetReclaimed.Match(line);
            if (match.Success)
            {
                rv = new PetEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    PetName = match.Groups["pet_name"].Value,
                    Incident = PetEvent.PetIncident.RECLAIMED
                };
            }

            // response to /pet leader
            match = regexPetLeader.Match(line);
            if (match.Success)
            {
                string leaderName = match.Groups["leader_name"].Value;

                // are we the pet leader?
                if (leaderName == activePlayer.Player.Name)
                {
                    rv = new PetEvent
                    {
                        Line = line,
                        TimeStamp = timestamp,
                        LineCounter = lineCounter,
                        PetName = match.Groups["pet_name"].Value,
                        Incident = PetEvent.PetIncident.LEADER
                    };
                }
            }

            // pet died
            match = regexPetDeath.Match(line);
            if (match.Success)
            {
                rv = new PetEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    PetName = match.Groups["pet_name"].Value,
                    Incident = PetEvent.PetIncident.DEATH
                };
            }

            // pet get lost
            match = regexPetGetLost.Match(line);
            if (match.Success)
            {
                rv = new PetEvent
                {
                    Line = line,
                    TimeStamp = timestamp,
                    LineCounter = lineCounter,
                    PetName = match.Groups["pet_name"].Value,
                    Incident = PetEvent.PetIncident.GETLOST
                };
            }

            return rv;
        }
    }
}
