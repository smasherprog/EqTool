using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;

namespace EQTool.Services.Spells.Log
{
    public class DPSLogParse
    {
        private readonly string PointsOfDamage = " points of damage.";
        private readonly string PointOfDamage = " point of damage.";
        private readonly string WasHitByNonMelee = "was hit by non-melee";
        private readonly ActivePlayer activePlayer;

        private readonly List<string> HitTypes = new List<string>()
        {
            " maul ",
            " mauls ",
            " strike ",
            " strikes ",
            " slice ",
            " slices ",
            " slash ",
            " slashes ",
            " crush ",
            " crushes ",
            " bite ",
            " bites ",
            " bash ",
            " bashes ",
            " kick ",
            " kicks ",
            " hit ",
            " hits ",
            " punch ",
            " punches ",
            " pierce ",
            " pierces ",
            " backstab ",
            " backstabs ",
            " claw ",
            " claws ",
            " gores "
        };

        public DPSLogParse(ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
        }

        public DPSParseMatch Match(string message, DateTime date)
        {
            if (message.Contains(WasHitByNonMelee))
            {
                return null;
            }

            if (message.EndsWith(PointsOfDamage) || message.EndsWith(PointOfDamage))
            {
                message = message.Replace(PointsOfDamage, string.Empty);
                message = message.Replace(PointOfDamage, string.Empty);
                var splits = message.Split(' ');
                var damagedone = splits[splits.Length - 1];
                var nameofattacker = string.Empty;
                var nameoftarget = string.Empty;
                var hittype = string.Empty;
                foreach (var item in HitTypes)
                {
                    var found = message.IndexOf(item);
                    if (found != -1)
                    {
                        hittype = item;
                        nameofattacker = message.Substring(0, found + 1).Trim();
                        nameoftarget = message.Replace(nameofattacker, string.Empty).Replace(item, string.Empty).Replace(damagedone, string.Empty).Replace(" for ", string.Empty).Trim();
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(nameofattacker))
                {
                    if (nameofattacker.ToLower() == "you" && activePlayer.Player != null && activePlayer.Player.PlayerClass == null)
                    {
                        if (hittype.Contains("backstab"))
                        {
                            activePlayer.Player.PlayerClass = PlayerClasses.Rogue;
                        }
                    }

                    return new DPSParseMatch
                    {
                        SourceName = nameofattacker,
                        DamageDone = int.Parse(damagedone),
                        TimeStamp = date,
                        TargetName = nameoftarget
                    };
                }
            }

            return null;
        }
    }
}
