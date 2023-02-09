using EQTool.Models;

namespace EQTool.Services.Spells.Log
{
    public class SpellWornOffLogParse
    {
        public string MatchSpell(string linelog)
        {
            var message = linelog.Substring(27);
            return message.StartsWith(EQSpells.Your) && message.EndsWith(EQSpells.SpellHasWornoff)
                ? message.Replace(EQSpells.Your, string.Empty).Replace(EQSpells.SpellHasWornoff, string.Empty).Trim()
                : null;
        }
    }
}
