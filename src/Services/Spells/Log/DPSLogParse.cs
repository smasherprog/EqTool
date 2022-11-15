using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EQTool.Services.Spells.Log
{
    public class DPSLogParse
    {
        private readonly string PointsOfDamage = " points of damage.";
        private readonly string PointOfDamage = " point of damage.";
        private readonly string WasHitByNonMelee = "was hit by non-melee";

        private readonly List<string> HitTypes = new List<string>()
        {
            " slice ",
            " slices ",
            " crush ",
            " crushes ",
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
            " backstabs "
        };

        public DPSLogParse()
        {
        }

        public EntittyDPS Match(string linelog)
        {
            var date = linelog.Substring(1, 25);
            if (DateTime.TryParse(date, out _))
            {

            }

            var message = linelog.Substring(27);
            Debug.WriteLine($"DPSParse: " + message);
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
                foreach (var item in HitTypes)
                {
                    var found = message.IndexOf(item);
                    if (found != -1)
                    {
                        nameofattacker = message.Substring(0, found + 1).Trim();
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(nameofattacker))
                {
                    return new EntittyDPS
                    {
                        Name = nameofattacker,
                        TotalDamage = int.Parse(damagedone)
                    };
                }
            }

            return null;
        }
    }
}
