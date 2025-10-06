using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class SpellViewModelTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private const string YouBeginCasting = "You begin casting ";
        private const string DummyEntryToForceEmitEvent = "You can't use that command right now...";

        public SpellViewModelTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
        }

        [TestMethod]
        public void NecroDA1()
        {
            logParser.Push("You are surrounded by the Quivering Veil of Xarn.", DateTime.Now);
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Quivering Veil of Xarn Cooldown") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 600.0);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Quivering Veil of Xarn") as SpellViewModel;
            spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Quivering Veil of Xarn") as SpellViewModel;
            Assert.AreEqual(spelleffect.TotalDuration.TotalSeconds, 18.0);
        }

        //this DOES NOT HAVE a landed message
        [TestMethod]
        public void NecroDA2()
        {
            logParser.Push(YouBeginCasting + " Harmshield.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(3));
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Harmshield Cooldown") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 599.0, 2);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Harmshield") as SpellViewModel;
            spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Harmshield") as SpellViewModel;
            Assert.AreEqual(spelleffect.TotalDuration.TotalSeconds, 17.0, 2);
        }

        [TestMethod]
        public void Harvest()
        {
            logParser.Push("You gather mana from your surroundings.", DateTime.Now);
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Harvest Cooldown") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 600.0);
            Assert.AreEqual(EQSpells.SpaceYou, dteffect.GroupName);
        }

        //this DOES NOT HAVE a landed message
        [TestMethod]
        public void ClericDA1()
        {
            logParser.Push(YouBeginCasting + " Divine Aura.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(3));
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Divine Aura Cooldown") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 899.0, 2);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Divine Aura") as SpellViewModel;
            Assert.AreEqual(spelleffect.TotalDuration.TotalSeconds, 17.0, 2);
        }

        [TestMethod]
        public void ClericDA2()
        {
            logParser.Push("You are surrounded by a divine barrier.", DateTime.Now);
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Divine Barrier Cooldown") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 900.0);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Divine Barrier") as SpellViewModel;
            Assert.AreEqual(spelleffect.TotalDuration.TotalSeconds, 18.0);
        }

        [TestMethod]
        public void ClericDA3()
        {
            logParser.Push("Someone is surrounded by a divine barrier.", DateTime.Now);
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Divine Barrier Cooldown") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 900.0);
            Assert.AreEqual(dteffect.GroupName, "Someone");
        }

        [TestMethod]
        public void Dictate()
        {
            logParser.Push("You begin casting Dictate.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(7));

            // expected = cast + recast - 7 = 5 + 300 - 7 = 298
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Dictate Cooldown") as TimerViewModel;
            Assert.AreEqual(298.0, dteffect.TotalDuration.TotalSeconds, 2);

            // exptected = cast + duration - 7 = 5 + 48 - 7 = 46
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Dictate") as SpellViewModel;
            Assert.AreEqual(46.0, spelleffect.TotalDuration.TotalSeconds, 2);
        }

        [TestMethod]
        public void Harmony()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Druid;
            player.Player.Level = 60;
            logParser.Push("You begin casting Harmony.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(4));

            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Harmony") as SpellViewModel;
            Assert.AreEqual(120, spelleffect.TotalDuration.TotalSeconds, 2);
        }

        [TestMethod]
        public void TOT()
        {
            logParser.Push("You begin casting Theft of Thought.", DateTime.Now);
            logParser.Push("Someone staggers.", DateTime.Now.AddSeconds(2));
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Theft of Thought Cooldown") as TimerViewModel;
            Assert.AreEqual(120, dteffect.TotalDuration.TotalSeconds, 2);
            Assert.AreEqual(dteffect.GroupName, EQSpells.SpaceYou);
        }

        [TestMethod]
        public void Frostreavers()
        {
            // two casts, but since this is a beneficial spell, should be 1 timer
            logParser.Push("You feel the blessing of ancient Coldain heroes.", DateTime.Now);
            logParser.Push("You feel the blessing of ancient Coldain heroes.", DateTime.Now);
            var spelleffect = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Frostreaver's Blessing") as SpellViewModel;
            Assert.IsNotNull(spelleffect);
            Assert.AreEqual(3600.0, spelleffect.TotalDuration.TotalSeconds);
            Assert.AreEqual(spelleffect.GroupName, EQSpells.SpaceYou);
            Assert.AreEqual(1, SpellList.Count);
        }
        private List<PersistentViewModel> SpellList => spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).ToList();
        private List<PersistentViewModel> TimerList => spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Timer).ToList();
        [TestMethod]
        public void Dazzle_StartNewTimer()
        {
            //[Sun Dec 15 19:39:13 2024] You begin casting Dazzle.
            //[Sun Dec 15 19:39:15 2024] Orc centurion has been mesmerized.

            // recast mode in StartNew
            // two casts, and since we are in StartNew mode, should be two timers
            player.Player.TimerRecastSetting = TimerRecast.StartNewTimer;
            logParser.Push("You begin casting Dazzle.", DateTime.Now);
            logParser.Push("Orc centurion has been mesmerized.", DateTime.Now.AddSeconds(2.0));
            logParser.Push("You begin casting Dazzle.", DateTime.Now.AddSeconds(10.0));
            logParser.Push("Orc centurion has been mesmerized.", DateTime.Now.AddSeconds(12.0));

            var spelleffect = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Dazzle") as SpellViewModel;
            Assert.IsNotNull(spelleffect);
            Assert.AreEqual(102.0, spelleffect.TotalDuration.TotalSeconds, .1);
            Assert.AreEqual(" Orc centurion", spelleffect.GroupName);
            Assert.AreEqual(2, SpellList.Count);
        }

        [TestMethod]
        public void Dazzle_RestartCurrentTimer()
        {
            //[Sun Dec 15 19:39:13 2024] You begin casting Dazzle.
            //[Sun Dec 15 19:39:15 2024] Orc centurion has been mesmerized.

            // recast mode in Restart
            // two casts, but since we are in Restart mode, should be just one timers
            player.Player.TimerRecastSetting = TimerRecast.RestartCurrentTimer;
            logParser.Push("You begin casting Dazzle.", DateTime.Now);
            logParser.Push("Orc centurion has been mesmerized.", DateTime.Now.AddSeconds(2.0));
            logParser.Push("You begin casting Dazzle.", DateTime.Now.AddSeconds(10.0));
            logParser.Push("Orc centurion has been mesmerized.", DateTime.Now.AddSeconds(12.0));

            var spelleffect = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Dazzle") as SpellViewModel;
            Assert.IsNotNull(spelleffect);
            Assert.AreEqual(102.0, spelleffect.TotalDuration.TotalSeconds, .1);
            Assert.AreEqual(" Orc centurion", spelleffect.GroupName);
            Assert.AreEqual(1, SpellList.Count);
        }

        [TestMethod]
        public void GuessUltravision()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Shaman;
            player.Player.Level = 45;
            logParser.Push("You begin casting Ultravision.", DateTime.Now);
            logParser.Push("Your eyes tingle.", DateTime.Now.AddSeconds(5));
            var spelleffect = SpellList.FirstOrDefault();
            Assert.AreEqual("Ultravision", spelleffect.Name);
        }

        [TestMethod]
        public void TestFluxStaff()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Wizard;
            player.Player.Level = 23;
            logParser.Push("A deepwater crocodile looks uncomfortable.", DateTime.Now);
            logParser.Push("A deepwater crocodile looks uncomfortable.", DateTime.Now.AddSeconds(5));
            var spelleffect = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Counter).Cast<CounterViewModel>().FirstOrDefault();
            Assert.AreEqual(spelleffect.Count, 2);
        }

        [TestMethod]
        public void GuessParalyzingEarch()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 60;
            logParser.Push("You begin casting Paralyzing Earth.", DateTime.Now);
            logParser.Push("A froglok dar knight hits YOU for 95 points of damage.", DateTime.Now);
            logParser.Push("A froglok dar knight hits YOU for 17 points of damage.", DateTime.Now.AddSeconds(2));
            logParser.Push("You regain your concentration and continue your casting.", DateTime.Now.AddSeconds(2));
            logParser.Push("A froglok dar knight's feet adhere to the ground.", DateTime.Now.AddSeconds(2));

            var spelleffect = SpellList.FirstOrDefault();
            Assert.AreEqual("Paralyzing Earth", spelleffect.Name);
        }

        [TestMethod]
        public void GuessDruidThorns()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Druid;
            player.Player.Level = 60;
            logParser.Push("You begin casting Legacy of Thorn.", DateTime.Now);
            logParser.Push("Someone is surrounded by a thorny barrier.", DateTime.Now.AddSeconds(3));

            var spelleffect = SpellList.FirstOrDefault();
            Assert.AreEqual("Legacy of Thorn", spelleffect.Name);
            Assert.AreEqual("Someone", spelleffect.GroupName);
        }

        [TestMethod]
        public void CloudOfSilence()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Druid;
            player.Player.Level = 60;
            player.Player.Zone = "growthplane";
            logParser.Push("You are in a cloud of silence.", DateTime.Now);

            var spelleffect = TimerList.FirstOrDefault();
            Assert.AreEqual("Cloud of Silence", spelleffect.Name);
        }


        [TestMethod]
        public void TestWakeOfTranq()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 60;
            var d = DateTime.Now;
            logParser.Push(spellWindowViewModel, "You begin casting Wake of Tranquility.", d);
            logParser.Push(spellWindowViewModel, "A wooly mammoth looks less aggressive.", d.AddSeconds(5));
            logParser.Push(spellWindowViewModel, "A tundra mammoth looks less aggressive.", d.AddSeconds(5));
            spellWindowViewModel.UpdateSpells(1000);
            var spelleffecst = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Wake of Tranquility").Cast<SpellViewModel>();
            Assert.AreEqual(spelleffecst.Count(), 2);
        }

        [TestMethod]
        public void TestRampage()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 60;
            var d = DateTime.Now;
            logParser.Push(spellWindowViewModel, "You feel the urge to rampage.", d);
            spellWindowViewModel.UpdateSpells(1000);
            var spelleffecst = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Rampage").Cast<SpellViewModel>();
            Assert.AreEqual(spelleffecst.Count(), 1);
        }

        [TestMethod]
        public void PreventMultipleSpellsFrombeingRemoved()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 60;
            var d = DateTime.Now;
            logParser.Push(spellWindowViewModel, "You begin casting Clinging Darkness.", d);
            logParser.Push(spellWindowViewModel, "A wooly mammoth is surrounded by darkness.", d.AddSeconds(2));
            logParser.Push(spellWindowViewModel, "You begin casting Clinging Darkness.", d.AddSeconds(10));
            logParser.Push(spellWindowViewModel, "A tundra mammoth is surrounded by darkness.", d.AddSeconds(13));
            logParser.Push(spellWindowViewModel, "Your Clinging Darkness spell has worn off.", d.AddSeconds(43));
            spellWindowViewModel.UpdateSpells(1000);
            var spelleffecst = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Clinging Darkness").Cast<SpellViewModel>();
            Assert.AreEqual(spelleffecst.Count(), 1);
        }

        [TestMethod]
        public void MakeSureDetrimentalSpellsShowMultipleTimes()
        {
            // two casts, and since we are in StartNew mode, should be two timers
            player.Player.TimerRecastSetting = TimerRecast.StartNewTimer;
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Rogue;
            player.Player.Level = 60;
            var d = DateTime.Now;
            logParser.Push(spellWindowViewModel, "A frost giant sentinel hits YOU for 35 points of damage.", d);
            logParser.Push(spellWindowViewModel, "A frost giant sentinel yawns.", d);
            logParser.Push(spellWindowViewModel, "A frost giant sentinel yawns.", d);

            Assert.AreEqual(SpellList.Count(), 2);
        }

        [TestMethod]
        public void SlainTest1()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Rogue;
            player.Player.Level = 60;

            logParser.Push("A Dizok Underling hits a Dizok Observer for 37 points of damage.", DateTime.Now);
            logParser.Push("A Dizok Underling hits a Dizok Observer for 121 points of damage.", DateTime.Now);
            logParser.Push("a Dizok Observer has been slain by a Dizok Underling!", DateTime.Now);
            logParser.Push("Your faction standing with GoblinsofMountainDeath got better.", DateTime.Now);
            logParser.Push("Your faction standing with SarnakCollective could not possibly get any worse.", DateTime.Now);
            logParser.Push("You gain party experience!!", DateTime.Now);
            logParser.Push("a Dizok Observer begins to shake as the mana burns within its body.", DateTime.Now);
            Assert.AreEqual(TimerList.Count(), 1);
            logParser.Push("A Dizok Underling hits a Dizok Observer for 37 points of damage.", DateTime.Now);
            logParser.Push("A Dizok Underling hits a Dizok Observer for 121 points of damage.", DateTime.Now);
            logParser.Push("a Dizok Observer has been slain by a Dizok Underling!", DateTime.Now);
            logParser.Push("Your faction standing with GoblinsofMountainDeath got better.", DateTime.Now);
            logParser.Push("Your faction standing with SarnakCollective could not possibly get any worse.", DateTime.Now);
            logParser.Push("You gain party experience!!", DateTime.Now);
            logParser.Push("a Dizok Observer begins to shake as the mana burns within its body.", DateTime.Now);
            Assert.AreEqual(TimerList.Count(), 2);
        }
    }
}
