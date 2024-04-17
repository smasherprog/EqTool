using EQTool.Models;
using EQTool.Services.Parsing;
using System.Collections.Generic;
using System.Linq;
using static EQTool.Services.EventsList;

namespace EQTool.Services.Spells.Log
{
    public class SpellWornOffLogParse : ILogParser
    {
        private readonly EQSpells spells;
        private readonly EventsList eventsList;

        public SpellWornOffLogParse(EQSpells spells, EventsList eventsList)
        {
            this.spells = spells;
            this.eventsList = eventsList;
        }

        private List<string> MatchWornOffSelfSpell(string message)
        {
            return spells.WornOffSpells.TryGetValue(message, out var pspells)
                ? pspells.Select(a => a.name).ToList()
                       : new List<string>();
        }

        private string MatchWornOffOtherSpell(string message)
        {
            return message.StartsWith(EQSpells.Your) && message.EndsWith(EQSpells.SpellHasWornoff)
                       ? message.Replace(EQSpells.Your, string.Empty).Replace(EQSpells.SpellHasWornoff, string.Empty).Trim()
                       : string.Empty;
        }

        public bool Evaluate(string line)
        {
            if (line == "The screams fade away.")
            {
                this.eventsList.Handle(new SpellWornOffOtherEventArgs { SpellName = "Soul Consumption" });
                return true;
            }
            var otherworeoff = MatchWornOffOtherSpell(line);
            if (string.IsNullOrWhiteSpace(otherworeoff))
            {
                var selfworoff = this.MatchWornOffSelfSpell(line);
                if (selfworoff.Any())
                {
                    this.eventsList.Handle(new SpellWornOffSelfEventArgs { SpellNames = selfworoff });
                    return true;
                }
            }
            else
            {
                this.eventsList.Handle(new SpellWornOffOtherEventArgs { SpellName = otherworeoff });
                return true;
            }

            return false;
        }
    }
}
