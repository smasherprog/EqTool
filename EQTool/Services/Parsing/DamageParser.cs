using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EQTool.Services.Parsing
{
    //
    // DamageParser
    //
    // Parse line for
    //      melee attacks from this player that land
    //      melee attacks from this player that miss
    //      melee attacks from other entities that land
    //      non-melee damage events
    //
    public class DamageParser : IEqLogParseHandler
    {
        private readonly ActivePlayer activePlayer;
        private readonly LogEvents logEvents;

        //
        // ctor
        //
        public DamageParser(ActivePlayer activePlayer, LogEvents logEvents)
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
        }

        // handle a line from the log file
        public bool Handle(string line, DateTime timestamp)
        {
            DamageEvent de = Match(line, timestamp);
            if (de != null)
            {
                logEvents.Handle(de);
                return true;
            }
            return false;
        }

        public DamageEvent Match(string line, DateTime timestamp)
        {
            // return value
            DamageEvent rv = null;

            // melee attack, hit or miss
            // https://regex101.com/r/77YkpV/1
            var meleePattern = @"^(?<attacker_name>[\w` ]+?) (try to )?(?<dmg_type>(hit(s)?|slash(es)?|pierce(s)?|crush(es)?|claw(s)?|bite(s)?|sting(s)?|maul(s)?|gore(s)?|punch(es)?|kick(s)?|backstab(s)?|bash(es)?|slice(s)?|strike(s)?)) (?<target_name>[\w` ]+)(( for (?<damage>[\d]+) point(s)? of damage)|(, but miss))";
            var meleeRegex = new Regex(meleePattern, RegexOptions.Compiled);
            var match = meleeRegex.Match(line);
            if (match.Success)
            {
                // the damage capture group will be an empty string if this was a miss
                var dmg = 0;
                if (match.Groups["damage"].Value != "")
                    dmg = int.Parse(match.Groups["damage"].Value);

                rv = new DamageEvent(timestamp, line, match.Groups["target_name"].Value, match.Groups["attacker_name"].Value, dmg, match.Groups["dmg_type"].Value);
            }

            // non-melee damage (direct damage spell, or dmg shield, or weapon proc)
            var nonMeleePattern = @"^(?<target_name>[\w` ]+) was hit by non-melee for (?<damage>[\d]+) point(s)? of damage";
            var nonMeleeRegex = new Regex(nonMeleePattern, RegexOptions.Compiled);
            match = nonMeleeRegex.Match(line);
            if (match.Success)
            {
                rv = new DamageEvent(timestamp, line, match.Groups["target_name"].Value, "You", int.Parse(match.Groups["damage"].Value), "non-melee");
            }

            // if we see a backstab from current player, set current player class to rogue
            if (rv != null && rv.AttackerName == "You" && activePlayer.Player != null && activePlayer.Player.PlayerClass == null)
            {
                if (rv.DamageType.Contains("backstab"))
                    activePlayer.Player.PlayerClass = PlayerClasses.Rogue;
            }

            return rv;
        }

        public enum PetLevel
        {
            Best,
            AboveAverage,
            Average,
            BelowAverage,
            Worst
        }

        public static PetLevel GetPetLevel(int hit, PlayerClasses playerClasses, int playerlevel)
        {
            if (playerClasses == PlayerClasses.Magician)
            {
                switch (hit)
                {
                    case 12 when playerlevel <= 4:
                    case 16 when playerlevel <= 8:
                    case 18 when playerlevel <= 12:
                    case 20 when playerlevel <= 16:
                    case 22 when playerlevel <= 20:
                    case 26 when playerlevel <= 24:
                    case 28 when playerlevel <= 29:
                    case 34 when playerlevel <= 34:
                    case 40 when playerlevel <= 39:
                    case 48 when playerlevel <= 44:
                    case 56 when playerlevel <= 49:
                    case 58 when playerlevel <= 51:
                    case 60 when playerlevel <= 57:
                        return PetLevel.Best;
                    case 11:
                        return PetLevel.AboveAverage;
                    case 10:
                        return PetLevel.Average;
                    case 9:
                        return PetLevel.BelowAverage;
                    case 8:
                        return PetLevel.Worst;
                }
            }

            return PetLevel.AboveAverage;
        }
    }
}
