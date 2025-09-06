﻿using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using EQTool.ViewModels.MobInfoComponents;
using EQTool.ViewModels.SpellWindow;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class SpellMatchingTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly SpellDurations spellDurations;
        private readonly EQSpells spells;
        private const string YouBeginCasting = "You begin casting ";

        public SpellMatchingTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            spellDurations = container.Resolve<SpellDurations>();
            spells = container.Resolve<EQSpells>();
            logParser = container.Resolve<LogParser>();
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(spells);
            Assert.IsNotNull(spells.AllSpells);
            Assert.IsTrue(spells.AllSpells.Any());
        }

        [TestMethod]
        public void Test1()
        {
            var spellname = "Acumen";
            var spellclass = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = YouBeginCasting + " " + spellname;
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Shaman;
            logParser.Push(line, DateTime.Now);
            line = "Joe " + spellclass.cast_on_other;
            logParser.Push(line, DateTime.Now);

            var effect = SpellList.FirstOrDefault();
            Assert.IsNotNull(effect);
        }

        [TestMethod]
        public void TestPacifyDuration()
        {
            var spellname = "Pacify";
            var spellclass = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = YouBeginCasting + " " + spellname;
            player.Player.Level = 50;
            player.Player.PlayerClass = PlayerClasses.Enchanter;
            logParser.Push(line, DateTime.Now);
            line = "Joe " + spellclass.cast_on_other;
            logParser.Push(line, DateTime.Now);

            var effect = SpellList.FirstOrDefault() as TimerViewModel;
            Assert.IsNotNull(effect);
            Assert.AreEqual(effect.TotalDuration.TotalSeconds, 210);
        }

        [TestMethod]
        public void TestRangerBindSight()
        {
            var spellname = "Bind Sight";
            var spellclass = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = YouBeginCasting + " " + spellname;
            player.Player.Level = 50;
            player.Player.PlayerClass = PlayerClasses.Ranger;
            logParser.Push(line, DateTime.Now);
            line = "Your sight is bound.";
            logParser.Push(line, DateTime.Now.AddSeconds(4));

            var effect = SpellList.FirstOrDefault();
            Assert.IsNotNull(effect);
            Assert.AreEqual(effect.TotalDuration.TotalSeconds, 660);
        }

        [TestMethod]
        public void TestBurnoutIII_ForPet()
        {
            var settings = container.Resolve<EQToolSettings>();
            settings.YouOnlySpells = true;
            var pet = container.Resolve<PetViewModel>();
            pet.PetName = "Xibab";
            var spellname = "Burnout III";
            var spellclass = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = YouBeginCasting + " " + spellname;
            player.Player.Level = 59;
            player.Player.PlayerClass = PlayerClasses.Magician;
            logParser.Push(line, DateTime.Now);
            line = "Xibab goes berserk.";
            logParser.Push(line, DateTime.Now.AddSeconds(6));
            spellWindowViewModel.UpdateSpells(1000);

            var effect = SpellList.FirstOrDefault();
            Assert.IsNotNull(effect);
            Assert.AreEqual(effect.ColumnVisibility, System.Windows.Visibility.Visible);
        }

        [TestMethod]
        public void TestHarvestCoolDown()
        {
            var spellname = "Harvest";
            var spellclass = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            player.Player.Level = 1;
            player.Player.PlayerClass = PlayerClasses.Bard;
            var line = "Joe " + spellclass.cast_on_other;
            logParser.Push(line, DateTime.Now);

            var effect = TimerList.FirstOrDefault();
            Assert.AreEqual("Joe", effect.GroupName);
            Assert.IsNotNull(effect);
        }

        [TestMethod]
        public void TestParseP99GrimAura()
        {
            var line = "8639^Grim Aura^PLAYER_1^^^^A dull aura covers your hand.^'s hand is covered with a dull aura.^The grim aura fades.^0^0^0^0^3000^2250^2250^3^270^0^25^3^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^10^0^0^0^0^0^0^0^0^0^0^0^2503^2108^-1^-1^-1^-1^1^1^1^1^-1^-1^-1^-1^102^100^100^100^100^100^100^100^100^100^100^100^0^1^0^0^2^254^254^254^254^254^254^254^254^254^254^254^6^25^5^-1^0^0^255^255^255^255^22^255^255^255^255^255^4^255^255^255^255^255^43^0^0^8^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^100^0^37^94^0^0^0^0^0^0^0^0^0^0^0^7^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^0^5^101^12^92^3^270^0^0^0^0^3^105^0^0^0^0^0^0^0^0^0^0^1^1^0^0^0^0^0^-1^0^0^0^1^0^0^1^1^^0";
            var grimaura = ParseSpells_spells_us.ParseP99Line(line);
            Assert.IsNotNull(grimaura);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_GrimAura()
        {
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, PlayerClasses.ShadowKnight, 30);
            Assert.AreEqual(ret, 30);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_Journeymansboots()
        {
            var spellname = "JourneymanBoots";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(spell, PlayerClasses.ShadowKnight, 35);
            Assert.AreEqual(ret, 35);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk30_Alliance()
        {
            var spellname = "Alliance";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spell1 = spells.AllSpells.Where(a => a.cast_on_other == spell.cast_on_other);
            var duration = SpellDurations.GetDuration_inSeconds(spell, PlayerClasses.ShadowKnight, 35);
            Assert.AreEqual(duration, 0);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlyManaSieve()
        {
            var spellname = "Mana Sieve";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spell1 = spells.AllSpells.Where(a => a.cast_on_other == spell.cast_on_other).ToList();
            Assert.AreEqual(1, spell1.Count);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk60_GrimAura()
        {
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, PlayerClasses.ShadowKnight, 60);
            Assert.AreEqual(ret, 60);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlySk60_NaltronMark()
        {
            var spellname = "Naltron's Mark";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var spell1 = spells.AllSpells.Where(a => a.cast_on_other == spell.cast_on_other).ToList();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.ShadowKnight;
            var ret = spellDurations.MatchClosestLevelToSpell(spell1, DateTime.Now);
            Assert.AreEqual(ret.name, "Symbol of Naltron");
        }

        [TestMethod]
        public void TestSpellMatchCorrectlynecro1_GrimAura_weird_but_Shouldhandle()
        {
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, PlayerClasses.Necromancer, 1);
            Assert.AreEqual(ret, 4);
        }

        [TestMethod]
        public void TestSpellMatchCorrectlynecro60_GrimAura()
        {
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, PlayerClasses.Necromancer, 60);
            Assert.AreEqual(ret, 60);
        }

        [TestMethod]
        public void TestSpellMatchCorrectly_Level60_GrimAura_NoClass()
        {
            var grimauraname = "Grim Aura";
            var grimaura = spells.AllSpells.FirstOrDefault(a => a.name == grimauraname);
            var ret = EQTool.Services.SpellDurations.MatchClosestLevelToSpell(grimaura, null, 60);
            Assert.AreEqual(ret, 60);
        }

        private List<SpellViewModel> SpellList => spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).Cast<SpellViewModel>().ToList();
        private List<PersistentViewModel> TimerList => spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Timer).ToList();
        private List<PersistentViewModel> CounterList => spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Counter).ToList();

        [TestMethod]
        public void TestWarriorDiscipline()
        {
            var line = "jobob assumes an evasive fighting style.";
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Cleric;
            logParser.Push(line, DateTime.Now);
            var spell = TimerList.FirstOrDefault();
            Assert.IsNotNull(spell);
            Assert.AreEqual("Evasive Discipline Cooldown", spell.Name);
        }

        [TestMethod]
        public void TestWarriorDisciplineDefensive()
        {
            var line = "You assume a defensive fighting style..";
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Warrior;
            logParser.Push(line, DateTime.Now);
            var spell = spellWindowViewModel.SpellList.ToList();
            Assert.IsNotNull(spell);
            //Assert.AreEqual("Defensive Discipline Cooldown", spell.Name);
        }

        [TestMethod]
        public void TestMonkVoidance()
        {
            var line = "You become untouchable.";
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Monk;
            logParser.Push(line, DateTime.Now);
            var spell = SpellList.FirstOrDefault();
            Assert.IsNotNull(spell);
            Assert.AreEqual(8, spell.TotalRemainingDuration.Seconds);
        }

        [TestMethod]
        public void TestBoonofTheGarouOther()
        {
            var spellname = "Boon of the Garou";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = $"jobob {spell.cast_on_other}";
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Cleric;
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault(a => a.Name == spellname);
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, spellname);
        }

        [TestMethod]
        public void TestBoonofTheGarouYouCast()
        {
            var spellname = "Boon of the Garou";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = $"{YouBeginCasting} {spellname}";
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Enchanter;
            logParser.Push(line, DateTime.Now);

            line = $"Jobob{spell.cast_on_other}";
            logParser.Push(line, DateTime.Now.AddMilliseconds(spell.casttime + 200));

            var timerText = $"{spellname} Cooldown";
            var spelltimer = TimerList.FirstOrDefault(a => a.Name == timerText) as TimerViewModel;
            Assert.IsNotNull(spelltimer);
            Assert.AreEqual(spelltimer.Name, timerText);

            var spellvm = SpellList.FirstOrDefault(a => a.Name == spellname);
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, spellname);
        }

        [TestMethod]
        public void TestManaSeive()
        {
            var line = "An ancient Frost guardian staggers in pain.";
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Enchanter
            };
            logParser.Push(line, DateTime.Now);
            var spell = CounterList.FirstOrDefault();
            var timerText = $"{spell.Name} Cooldown";
            Assert.IsNotNull(spell);
            Assert.AreEqual("Mana Sieve Cooldown", timerText);
        }

        [TestMethod]
        public void TestBlackPomFlower()
        {
            var line = "Someone is covered by an aura of black petals.";
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Enchanter
            };
            logParser.Push(line, DateTime.Now);
            var spell = SpellList.FirstOrDefault();
            Assert.IsNotNull(spell);
            Assert.AreEqual(spell.Name, "Aura of Black Petals");
        }

        [TestMethod]
        public void TestClericAego()
        {
            var aego = "Aegolism";
            var aegospell = spells.AllSpells.FirstOrDefault(a => a.name == aego);
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            logParser.Push(aegospell.cast_on_other, DateTime.Now);
            var spell = SpellList.FirstOrDefault();
            Assert.IsNotNull(spell);
            Assert.AreEqual(spell.Name, aego);
        }

        [TestMethod]
        public void TestCallOfThePredetorAego()
        {
            var spellname = "Call of the Predator";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };

            logParser.Push("Jobob " + spell.cast_on_other, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spell);
            Assert.AreEqual(spellvm.Name, spellname);
        }

        [TestMethod]
        public void TestResistMagic()
        {
            var spellname = "Group Resist Magic";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            logParser.Push("Jobob " + spell.cast_on_other, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
        }

        [TestMethod]
        public void TestSavageSpirit()
        {
            var spellname = "Savage Spirit";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            logParser.Push("Jobob " + spell.cast_on_other, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
        }

        [TestMethod]
        public void TestSpeedOfShissar()
        {
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            logParser.Push("Jobob " + shissarspell.cast_on_other, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, shissar);
        }

        [TestMethod]
        public void TestSpeedOfShissar1()
        {
            var spells = container.Resolve<EQSpells>();
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "Jobober " + shissarspell.cast_on_other;
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, shissar);
        }

        [TestMethod]
        public void TestSlowForShadowKnight()
        {
            var shissar = "Turgur's Insects";
            player.Player.Level = 54;
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "Jobober " + shissarspell.cast_on_other;
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, shissar);
            Assert.AreEqual(5.4, spellvm.TotalDuration.TotalMinutes, .1);
        }

        [TestMethod]
        public void TestUserBeginsToCastUserCastingSpell()
        {
            var spellname = "Burnout";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = $"You begin casting {spellname}.";
            player.Player = new PlayerInfo
            {
                Level = 24,
                PlayerClass = PlayerClasses.Magician
            };

            logParser.Push(line, DateTime.Now);
            line = "Jobober " + shissarspell.cast_on_other;
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
        }

        [TestMethod]
        public void TestSlowForMagePetHaste1()
        {
            var spellname = "Burnout";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "Jobober " + shissarspell.cast_on_other;
            player.Player.Level = 21;
            player.Player.PlayerClass = PlayerClasses.Magician;
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
        }

        [TestMethod]
        public void TestSlowForNecro()
        {
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "Jobober " + shissarspell.cast_on_other;
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(6, spellvm.TotalDuration.TotalMinutes, .2);
        }

        [TestMethod]
        public void TestFluxStaff()
        {
            var spellname = "LowerElement";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "Jobober " + spell.cast_on_other;
            player.Player.Level = 52;
            player.Player.PlayerClass = PlayerClasses.Warrior;
            logParser.Push(line, DateTime.Now);
            var spellvm = CounterList.FirstOrDefault() as CounterViewModel;
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(1, spellvm.Count);
        }

        [TestMethod]
        public void TestSlowForNecro_Multiname_onespace()
        {
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober " + shissarspell.cast_on_other;
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(6, spellvm.TotalDuration.TotalMinutes, .2);
            Assert.AreEqual(spellvm.Name, spellname);
            Assert.AreEqual(spellvm.GroupName, "an Jobober");
        }

        [TestMethod]
        public void TestManicalStrength()
        {
            var spellname = "Manicial Strength";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober " + shissarspell.cast_on_other;
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Shaman;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(144, spellvm.TotalDuration.TotalMinutes);
            Assert.AreEqual(spellvm.Name, spellname);
            Assert.AreEqual(spellvm.GroupName, "an Jobober");
        }

        [TestMethod]
        public void TestSlowForNecro_Multiname_twospace()
        {
            var spellname = "Turgur's Insects";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober rager " + shissarspell.cast_on_other;
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(6, spellvm.TotalDuration.TotalMinutes, .2);
            Assert.AreEqual(spellvm.Name, spellname);
            Assert.AreEqual(spellvm.GroupName, "an Jobober rager");
        }

        [TestMethod]
        public void TestSlowForNecro_Multiname_threespace()
        {
            var spellname = "Cripple";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = "an Jobober rager " + shissarspell.cast_on_other;
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(7, spellvm.TotalDuration.TotalMinutes, .2);
            Assert.AreEqual(spellvm.Name, spellname);
            Assert.AreEqual(spellvm.GroupName, "an Jobober rager");
        }

        [TestMethod]
        public void TestSpeedOfShissar2()
        {
            var shissar = "Speed of the Shissar";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A bottomless feaster's body pulses with the spirit of the Shissar.";
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Cleric;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, shissar);
            Assert.AreEqual(spellvm.GroupName, " A bottomless feaster");
        }

        [TestMethod]
        public void TestShamanEpic()
        {
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A Ratling is consumed by the raging spirits of the land.";
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, shissar);
            Assert.AreEqual(spellvm.GroupName, " A Ratling");
        }

        [TestMethod]
        public void TestShamanEpic2()
        {
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "Gkrean Prophet of Tallon is consumed by the raging spirits of the land.";
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, shissar);
            Assert.AreEqual(spellvm.GroupName, " Gkrean Prophet of Tallon");
        }

        [TestMethod]
        public void TestShamanEpic1()
        {
            var shissar = "Curse of the Spirits";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A Ratling is consumed by the raging spirits of the land.";
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Necromancer;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, shissar);
            Assert.AreEqual(spellvm.GroupName, " A Ratling");
        }

        [TestMethod]
        public void TestShamanUsingTashStick()
        {
            var shissar = "Tashanian";
            var shissarspell = spells.AllSpells.FirstOrDefault(a => a.name == shissar);
            var line = "A Mangy Rat glances nervously about.";
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Shaman;

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.Name, shissar);
            Assert.AreEqual(spellvm.GroupName, " A Mangy Rat");
        }

        [TestMethod]
        public void TestWarriorDisciplineGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "joe assumes an evasive fighting style.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);

            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell);
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.GroupName, "joe");
        }

        [TestMethod]
        public void TestClericAegolineGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var aego = "Aegolism";
            var aegospell = spells.AllSpells.FirstOrDefault(a => a.name == aego);
            _ = spells.CastOtherSpells.TryGetValue(aegospell.cast_on_other, out var spells1);
            player.Player.PlayerClass = PlayerClasses.Cleric;
            player.Player.Level = 54;
            var foundspell = spellDurations.MatchClosestLevelToSpell(spells1, DateTime.Now);
            Assert.IsNotNull(foundspell);
            Assert.AreEqual(aego, foundspell.name);
        }

        [TestMethod]
        public void TestBurnout()
        {
            var spells = container.Resolve<EQSpells>();
            var spellname = "Burnout";
            var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            _ = spells.CastOtherSpells.TryGetValue(spell.cast_on_other, out var spells1);
            player.Player.PlayerClass = PlayerClasses.Magician;
            player.Player.Level = 14;
            var foundspell = spellDurations.MatchClosestLevelToSpell(spells1, DateTime.Now);
            Assert.IsNotNull(foundspell);
            Assert.AreEqual(spellname, foundspell.name);
        }

        [TestMethod]
        public void TestClairityGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            player.Player.PlayerClass = PlayerClasses.Cleric;
            player.Player.Level = 54;
            var foundspell = spellDurations.MatchClosestLevelToSpell(spells1, DateTime.Now);
            Assert.IsNotNull(foundspell);
            Assert.AreEqual("Clarity II", foundspell.name);
        }

        [TestMethod]
        public void TestClairityDurationGuess()
        {
            var spells = container.Resolve<EQSpells>();
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            player.Player.PlayerClass = PlayerClasses.Cleric;
            player.Player.Level = 54;
            var foundspell = spellDurations.MatchClosestLevelToSpell(spells1, DateTime.Now);
            var duration = SpellDurations.GetDuration_inSeconds(foundspell, PlayerClasses.Cleric, 54);
            Assert.IsNotNull(foundspell);
            Assert.IsNotNull(duration);
            Assert.AreEqual("Clarity II", foundspell.name);
        }

        [TestMethod]
        public void TestClairityDurationGuess1()
        {
            var line = "A soft breeze slips through your mind.";
            var player = container.Resolve<ActivePlayer>();
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Cleric;
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
        }

        [TestMethod]
        public void TestHealForDamage()
        {
            var line = "You mend your wounds and heal some damage.";
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
        }

        [TestMethod]
        public void TestRing8()
        {
            var line = "You are enveloped in a swirling maelstrom.";
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            logParser.Push(line, DateTime.Now);
            var spellvm = SpellList.FirstOrDefault();
            Assert.IsNotNull(spellvm);
            Assert.AreEqual(spellvm.TotalDuration.TotalSeconds, 3600);
        }

        [TestMethod]
        public void TestClairityDurationGuess_part1()
        {
            var line = "looks very tranquil.";
            _ = spells.CastOtherSpells.TryGetValue(line, out var spells1);
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var foundspell = spellDurations.MatchClosestLevelToSpell(spells1, DateTime.Now);
            var foundlevel = SpellDurations.MatchClosestLevelToSpell(foundspell, PlayerClasses.Cleric, 54);
            var duration = SpellDurations.GetDuration_inSeconds(foundspell, PlayerClasses.Cleric, 54);
            Assert.AreEqual(duration, 2100);
            Assert.AreEqual(foundlevel, 54);
        }

        [TestMethod]
        public void DateParseTest()
        {
            var line = "Sat Oct 08 11:31:38 2022";
            var d = LogFileDateTimeParse.ParseDateTime(line);
            Assert.AreEqual(d.ToString("M'/'d'/'yyyy hh:mm:ss tt"), "10/8/2022 11:31:38 AM");
        }

    }
}
