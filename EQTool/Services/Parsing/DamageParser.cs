using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
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

        //https://regex101.com/r/JPpEcr/1
        private const string youHitPattern = @"^You (?<dmg_type>hit|slash|pierce|crush|claw|bite|sting|maul|gore|punch|kick|backstab|bash|slice|strike) (?<target_name>[\w` ]+) for (?<damage>[\d]+) point(s)? of damage";

        //https://regex101.com/r/nvSnKN/1        
        private const string youMissPattern = @"^You try to (?<dmg_type>hit|slash|pierce|crush|claw|bite|sting|maul|gore|punch|kick|backstab|bash|slice|strike) (?<target_name>[\w` ]+), but";

        //https://regex101.com/r/PJfNGm/1        
        private const string otherHitPattern = @"^(?<attacker_name>[\w` ]+?) (?<dmg_type>hits|slashes|pierces|crushes|claws|bites|stings|mauls|gores|punches|kicks|backstabs|bashes|slices|strikes) (?<target_name>[\w` ]+) for (?<damage>[\d]+) point(s)? of damage";

        //https://regex101.com/r/5oJEoN/1
        private const string othersMissPattern = @"^(?<attacker_name>[\w` ]+?) tries to (?<dmg_type>hit|slash|pierce|crush|claw|bite|sting|maul|gore|punch|kick|backstab|bash|slice|strike) (?<target_name>[\w` ]+), but";
        
        private const string nonMeleePattern = @"^(?<target_name>[\w` ]+) was hit by non-melee for (?<damage>[\d]+) point(s)? of damage";

        private readonly Regex youHitRegex = new Regex(youHitPattern, RegexOptions.Compiled);
        private readonly Regex youMissRegex = new Regex(youMissPattern, RegexOptions.Compiled);
        private readonly Regex otherHitRegex = new Regex(otherHitPattern, RegexOptions.Compiled);
        private readonly Regex othersMissRegex = new Regex(othersMissPattern, RegexOptions.Compiled);
        private readonly Regex nonMeleeRegex = new Regex(nonMeleePattern, RegexOptions.Compiled);

        //
        // ctor
        //
        public DamageParser(ActivePlayer activePlayer, LogEvents logEvents)
        {
            this.activePlayer = activePlayer;
            this.logEvents = logEvents;
        }

        // handle a line from the log file
        public bool Handle(string line, DateTime timestamp, int lineCounter)
        {
            var de = Match(line, timestamp, lineCounter);
            if (de != null)
            {
                de.LineCounter = lineCounter;
                de.Line = line;
                de.TimeStamp = timestamp;
                logEvents.Handle(de);
                return true;
            }
            return false;
        }

        public DamageEvent Match(string line, DateTime timestamp, int lineCounter)
        {
            // you hit
            var match = youHitRegex.Match(line);
            if (match.Success)
            {
                var rv = new DamageEvent
                {
                    Line = line,
                    LineCounter = lineCounter,
                    TimeStamp = timestamp,
                    TargetName = match.Groups["target_name"].Value,
                    AttackerName = "You",
                    DamageDone = int.Parse(match.Groups["damage"].Value),
                    DamageType = match.Groups["dmg_type"].Value
                };

                // if we see a backstab from current player, set current player class to rogue
                if (rv.AttackerName == "You" && activePlayer.Player?.PlayerClass != PlayerClasses.Rogue)
                {
                    if (rv.DamageType.Contains("backstab"))
                    {
                        logEvents.Handle(new ClassDetectedEvent { TimeStamp = timestamp, Line = line, PlayerClass = PlayerClasses.Rogue });
                    }
                }

                return rv;
            }

            // you miss
            match = youMissRegex.Match(line);
            if (match.Success)
            {
                return new DamageEvent
                {
                    Line = line,
                    LineCounter = lineCounter,
                    TimeStamp = timestamp,
                    TargetName = match.Groups["target_name"].Value,
                    AttackerName = "You",
                    DamageDone = 0,
                    DamageType = match.Groups["dmg_type"].Value
                };
            }

            // others hit
            match = otherHitRegex.Match(line);
            if (match.Success)
            {
                return new DamageEvent
                {
                    Line = line,
                    LineCounter = lineCounter,
                    TimeStamp = timestamp,
                    TargetName = match.Groups["target_name"].Value,
                    AttackerName = match.Groups["attacker_name"].Value,
                    DamageDone = int.Parse(match.Groups["damage"].Value),
                    DamageType = match.Groups["dmg_type"].Value
                };
            }

            // others miss
            match = othersMissRegex.Match(line);
            if (match.Success)
            {
                return new DamageEvent(timestamp,
                                         line,
                                         match.Groups["target_name"].Value,
                                         match.Groups["attacker_name"].Value,
                                         0,
                                         match.Groups["dmg_type"].Value);
            }

            // non-melee damage (direct damage spell, or dmg shield, or weapon proc)
            match = nonMeleeRegex.Match(line);
            return match.Success
                ? new DamageEvent
                {
                    Line = line,
                    LineCounter = lineCounter,
                    TimeStamp = timestamp,
                    TargetName = match.Groups["target_name"].Value,
                    AttackerName = "You",
                    DamageDone = int.Parse(match.Groups["damage"].Value),
                    DamageType = "non-melee"
                }
                : null;
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
