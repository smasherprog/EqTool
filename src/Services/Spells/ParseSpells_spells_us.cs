using EQTool.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EQTool.Services
{
    public class ParseSpells_spells_us
    {
        private List<SpellBase> _Spells = new List<SpellBase>();
        private readonly EQToolSettings settings;

        public ParseSpells_spells_us(EQToolSettings settings)
        {
            this.settings = settings;
        }

        public List<SpellBase> GetSpells()
        {
            if (_Spells.Any())
            {
                return _Spells;
            }
            var spells = new Dictionary<string, SpellBase>();
            var spellastext = File.ReadAllLines(settings.DefaultEqDirectory + "/spells_us.txt");
            var skippedcounter = 0;
            foreach (var item in spellastext)
            {
                var spell = ParseLine(item);
                if (spell.name.StartsWith("GM "))
                {
                    continue;
                }

                if (spell.name.StartsWith("Guide "))
                {
                    continue;
                }

                if (spell.name.StartsWith("NPC"))
                {
                    continue;
                }

                if (spell.cast_on_you.ToLower().Contains("You feel quite amicable."))
                {
                    Debug.WriteLine($"spell.name");
                }

                if (spells.ContainsKey(spell.name))
                {
                    skippedcounter++;
                    spells[spell.name] = spell;
                }
                else
                {
                    spells.Add(spell.name, spell);
                }
            }
            Debug.WriteLine($"Skipped {skippedcounter}");

            _Spells = spells.Values.ToList();
            return _Spells;
        }

        public static SpellBase ParseLine(string line)
        {
            var splits = line.Split('^');
            var classes = new Dictionary<PlayerClasses, int>();
            for (var i = 104; i < 104 + (int)PlayerClasses.Enchanter + 1; i++)
            {
                if (int.TryParse(splits[i], out var l))
                {
                    if (l >= 0 && l < 255)
                    {
                        var clas = (PlayerClasses)(i - 104);
                        classes.Add(clas, l);
                    }
                }
            }

            return new SpellBase
            {
                id = int.Parse(splits[0]),
                name = splits[1],
                buffduration = int.Parse(splits[17]),
                buffdurationformula = int.Parse(splits[16]),
                pvp_buffdurationformula = int.Parse(splits[181]),
                type = int.Parse(splits[83]),
                cast_on_other = splits[7].Trim(),
                casttime = int.Parse(splits[13]),
                cast_on_you = splits[6].Trim(),
                spell_fades = splits[8].Trim(),
                Classes = classes,
                spell_icon = int.Parse(splits[144])
            };
        }
    }
}
