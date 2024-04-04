using EQTool.Models;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Spells.Log
{
    public class SpellWornOffLogParse
    {
        private readonly EQSpells spells;
        public SpellWornOffLogParse(EQSpells spells)
        {
            this.spells = spells;
        }

        public List<string> MatchWornOffSelfSpell(string message)
        {
            return spells.WornOffSpells.TryGetValue(message, out var pspells)
                ? pspells.Select(a => a.name).ToList()
                       : new List<string>();
        }

        public string MatchWornOffOtherSpell(string message)
        {
            return message.StartsWith(EQSpells.Your) && message.EndsWith(EQSpells.SpellHasWornoff)
                       ? message.Replace(EQSpells.Your, string.Empty).Replace(EQSpells.SpellHasWornoff, string.Empty).Trim()
                       : string.Empty;
        }
    }
}
