using EQTool.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EQTool.Services
{
    public class ParseSpells
    {
        private List<SpellBase> _Spells = new List<SpellBase>();
        private readonly EQToolSettings settings;

        public ParseSpells(EQToolSettings settings)
        {
            this.settings = settings;
        }

        public List<SpellBase> GetSpells()
        {
            if (_Spells.Any())
            {
                return _Spells;
            }

            var ret = new List<SpellBase>();
            var spellastext = File.ReadAllLines(settings.DefaultEqDirectory + "/spells_us.txt");
            foreach (var item in spellastext)
            {
                var splits = item.Split('^');
                var levelofspell = 0;
                for (var i = 105; i < 120; i++)
                {
                    if (int.TryParse(splits[i], out var l))
                    {
                        if (l >= levelofspell && l < 255)
                        {
                            levelofspell = l;
                        }
                    }
                }

                var spell = new SpellBase
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
                    Level = levelofspell,
                    spell_icon = int.Parse(splits[144])
                };
                ret.Add(spell);
                if (spell.cast_on_other.Contains("feet feel quick"))
                {
                    Debug.WriteLine("sdfsdf");
                }
                if (spell.cast_on_you.Contains("feet feel quick"))
                {
                    Debug.WriteLine("sdfsdf");
                }
                if (spell.name.Contains("Protection of the "))
                {
                    Debug.WriteLine("sdfsdf");
                }
            }
            _Spells = ret;
            return ret;
        }
    }
}
