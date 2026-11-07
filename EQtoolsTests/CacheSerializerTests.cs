using EQTool.Models;
using EQTool.Services;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace EQtoolsTests
{
    [TestClass]
    public class CacheSerializerTests
    {
        [TestMethod]
        public void SpellCacheRoundTrips()
        {
            var spells = new List<SpellBase>
            {
                new SpellBase
                {
                    id = 42,
                    name = "Summon Companion",
                    cast_on_you = "You feel a tug.",
                    cast_on_other = " looks around.",
                    spell_fades = "The tug fades.",
                    benefit_detriment = SpellBenefitDetriment.Beneficial,
                    casttime = 3000,
                    recastTime = 1500,
                    Classes = new Dictionary<PlayerClasses, int> { { PlayerClasses.Cleric, 4 }, { PlayerClasses.Paladin, 9 } },
                    buffdurationformula = 3,
                    buffduration = 270,
                    spell_icon = 17,
                    ResistCheck = 1,
                    DescrNumber = DescrNumber.Agility,
                    resisttype = ResistType.MAGIC,
                    SpellType = SpellType.GroupV1,
                    PetReagents = new List<Tuple<PetReagent, int>> { Tuple.Create(PetReagent.BoneChip, 2) }
                }
            };

            var copy = RoundTrip(spells);

            Assert.HasCount(1, copy);
            var spell = copy[0];
            Assert.AreEqual(42, spell.id);
            Assert.AreEqual("Summon Companion", spell.name);
            Assert.AreEqual(" looks around.", spell.cast_on_other);
            Assert.AreEqual(SpellBenefitDetriment.Beneficial, spell.benefit_detriment);
            Assert.AreEqual(1500, spell.recastTime);
            Assert.AreEqual(9, spell.Classes[PlayerClasses.Paladin]);
            Assert.AreEqual(270, spell.buffduration);
            Assert.AreEqual(DescrNumber.Agility, spell.DescrNumber);
            Assert.AreEqual(ResistType.MAGIC, spell.resisttype);
            Assert.AreEqual(SpellType.GroupV1, spell.SpellType);
            Assert.AreEqual(Tuple.Create(PetReagent.BoneChip, 2), spell.PetReagents[0]);
        }

        private static T RoundTrip<T>(T value)
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                CacheSerializer.WriteToFile(path, value);
                return CacheSerializer.ReadFromFile<T>(path);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
