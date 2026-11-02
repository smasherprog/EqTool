using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Map;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;

namespace EQtoolsTests
{
    [TestClass]
    public class CacheSerializerTests
    {
        [TestMethod]
        public void MapCacheRoundTrips()
        {
            var aabb = new AABB();
            aabb.Add(new Point3D(-10.5, 20.25, 3));
            aabb.Add(new Point3D(40, -5, 7.125));
            var map = new ParsedData
            {
                Lines = new List<MapLine>
                {
                    new MapLine { Points = new[] { new Point3D(1, 2, 3), new Point3D(-4.5, 5.25, -6) }, Color = new Colour(255, 10, 20, 30) }
                },
                Labels = new List<MapLabel>
                {
                    new MapLabel { Point = new Point3D(7, 8, 9), Color = new Colour(128, 1, 2, 3), label = "Zone_In", LabelSize = LabelSize.Large }
                },
                AABB = aabb,
                Offset = new Point3D(100, 200, 0)
            };

            var copy = RoundTrip(map);

            Assert.HasCount(1, copy.Lines);
            CollectionAssert.AreEqual(map.Lines[0].Points, copy.Lines[0].Points);
            Assert.AreEqual(map.Lines[0].Color, copy.Lines[0].Color);
            Assert.HasCount(1, copy.Labels);
            Assert.AreEqual(map.Labels[0].Point, copy.Labels[0].Point);
            Assert.AreEqual(map.Labels[0].Color, copy.Labels[0].Color);
            Assert.AreEqual("Zone_In", copy.Labels[0].label);
            Assert.AreEqual(LabelSize.Large, copy.Labels[0].LabelSize);
            Assert.AreEqual(aabb.Min, copy.AABB.Min);
            Assert.AreEqual(aabb.Max, copy.AABB.Max);
            Assert.AreEqual(map.Offset, copy.Offset);
        }

        [TestMethod]
        public void EmptyMapCacheRoundTrips()
        {
            var copy = RoundTrip(new ParsedData());

            Assert.IsEmpty(copy.Lines);
            Assert.AreEqual(new AABB().Min, copy.AABB.Min);
            Assert.AreEqual(new AABB().Max, copy.AABB.Max);
            Assert.AreEqual(600, copy.AABB.MaxHeight);
        }

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
