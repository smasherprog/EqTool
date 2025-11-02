using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Parsing;
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
        private List<PersistentViewModel> SpellList => spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell).ToList();
        private List<PersistentViewModel> TimerList => spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Timer).ToList();

        public SpellViewModelTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
        }

        [TestMethod]
        public void NecroDA1()
        {
            logParser.Push("You are surrounded by the Quivering Veil of Xarn.", DateTime.Now);
            
            var cooldown = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Quivering Veil of Xarn Cooldown") as SpellViewModel;
            Assert.IsNotNull(cooldown);
            Assert.AreEqual(600.0, cooldown.TotalDuration.TotalSeconds);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Target);
            
            var spellEffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Quivering Veil of Xarn") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(18.0, spellEffect.TotalDuration.TotalSeconds);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Target);
        }

        //this DOES NOT HAVE a landed message
        [TestMethod]
        public void NecroDA2()
        {
            logParser.Push(YouBeginCasting + " Harmshield.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(3));
            
            var cooldown = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Harmshield Cooldown") as SpellViewModel;
            Assert.IsNotNull(cooldown);
            Assert.AreEqual(599.0, cooldown.TotalDuration.TotalSeconds, 2);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Target);
            
            var spellEffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Harmshield") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(17.0, spellEffect.TotalDuration.TotalSeconds, 2);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Target);
        }

        [TestMethod]
        public void Harvest()
        {
            logParser.Push("You gather mana from your surroundings.", DateTime.Now);
            
            var cooldown = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Harvest Cooldown") as SpellViewModel;
            Assert.IsNotNull(cooldown);
            Assert.AreEqual(600.0, cooldown.TotalDuration.TotalSeconds);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Target);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Target);
        }

        //this DOES NOT HAVE a landed message
        [TestMethod]
        public void ClericDA1()
        {
            logParser.Push(YouBeginCasting + " Divine Aura.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(3));
            
            var cooldown = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Divine Aura Cooldown") as SpellViewModel;
            Assert.IsNotNull(cooldown);
            Assert.AreEqual(899.0, cooldown.TotalDuration.TotalSeconds, 2);
            
            var spellEffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Divine Aura") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(17.0, spellEffect.TotalDuration.TotalSeconds, 2);
        }

        [TestMethod]
        public void ClericDA2()
        {
            logParser.Push("You are surrounded by a divine barrier.", DateTime.Now);
            
            var cooldown = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Divine Barrier Cooldown") as SpellViewModel;
            Assert.IsNotNull(cooldown);
            Assert.AreEqual(900.0, cooldown.TotalDuration.TotalSeconds);
            
            var spellEffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Divine Barrier") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(18.0, spellEffect.TotalDuration.TotalSeconds);
        }

        [TestMethod]
        public void ClericDA3()
        {
            logParser.Push("Someone is surrounded by a divine barrier.", DateTime.Now);
            
            var cooldown = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Divine Barrier Cooldown") as SpellViewModel;
            Assert.IsNotNull(cooldown);
            Assert.AreEqual(900.0, cooldown.TotalDuration.TotalSeconds);
            Assert.AreEqual("Someone", cooldown.Target);
        }

        [TestMethod]
        public void Dictate()
        {
            logParser.Push("You begin casting Dictate.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(7));

            // expected = cast + recast - 7 = 5 + 300 - 7 = 298
            var cooldown = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Dictate Cooldown") as SpellViewModel;
            Assert.IsNotNull(cooldown);
            Assert.AreEqual(298.0, cooldown.TotalDuration.TotalSeconds, 2);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Target);

            // exptected = cast + duration - 7 = 5 + 48 - 7 = 46
            var spellEffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Dictate") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(46.0, spellEffect.TotalDuration.TotalSeconds, 2);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Target);
        }

        [TestMethod]
        public void Harmony()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Druid;
            player.Player.Level = 60;
            logParser.Push("You begin casting Harmony.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(5));
           
            var spellEffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Harmony") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(120, spellEffect.TotalDuration.TotalSeconds, 4);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
        }

        [TestMethod]
        public void TOT()
        {
            logParser.Push("You begin casting Theft of Thought.", DateTime.Now);
            logParser.Push("Someone staggers.", DateTime.Now.AddSeconds(2));
            
            var cooldown = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Theft of Thought Cooldown") as SpellViewModel;
            Assert.IsNotNull(cooldown);
            Assert.AreEqual(120, cooldown.TotalDuration.TotalSeconds, 2);
            Assert.AreEqual(EQSpells.SpaceYou, cooldown.Caster);
        }

        [TestMethod]
        public void Frostreavers()
        {
            // two casts, but since this is a beneficial spell, should be 1 timer
            logParser.Push("You feel the blessing of ancient Coldain heroes.", DateTime.Now);
            logParser.Push("You feel the blessing of ancient Coldain heroes.", DateTime.Now);
            
            var spellEffect = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Frostreaver's Blessing") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.HasCount(1, SpellList);
            Assert.AreEqual(3600.0, spellEffect.TotalDuration.TotalSeconds);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Target);
        }
        
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

            var spellEffect = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Dazzle") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.HasCount(2, SpellList);
            Assert.AreEqual(102.0, spellEffect.TotalDuration.TotalSeconds, .1);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual(" Orc centurion", spellEffect.Target);
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

            var spellEffect = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Dazzle") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.HasCount(1, SpellList);
            Assert.AreEqual(102.0, spellEffect.TotalDuration.TotalSeconds, .1);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual(" Orc centurion", spellEffect.Target);
        }

        [TestMethod]
        public void GuessUltravision()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Shaman;
            player.Player.Level = 45;
            logParser.Push("You begin casting Ultravision.", DateTime.Now);
            logParser.Push("Your eyes tingle.", DateTime.Now.AddSeconds(5));
            
            var spellEffect = SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Ultravision") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Target);
            Assert.AreEqual("Ultravision", spellEffect.Id);
        }

        [TestMethod]
        public void TestFluxStaff()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Wizard;
            player.Player.Level = 23;
            logParser.Push("A deepwater crocodile looks uncomfortable.", DateTime.Now);
            logParser.Push("A deepwater crocodile looks uncomfortable.", DateTime.Now.AddSeconds(5));
            
            var spellEffect = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Counter).Cast<CounterViewModel>().FirstOrDefault();
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(2, spellEffect.Count);
            Assert.AreEqual(string.Empty, spellEffect.Caster);
            Assert.AreEqual(" A deepwater crocodile", spellEffect.Target);
        }

        [TestMethod]
        public void GuessDruidEpic()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 60;
            player.Player.Zone = "skyfire";
            logParser.Push("A wyvern has been gripped by nature's wrath.", DateTime.Now); 

            var spellEffect = spellWindowViewModel.SpellList.FirstOrDefault(a=> a.Id == "Wrath of Nature") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(string.Empty, spellEffect.Caster);
            Assert.AreEqual(" A wyvern", spellEffect.Target);
        }

        [TestMethod]
        public void GuessParalyzingEarth()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 60;
            logParser.Push("You begin casting Paralyzing Earth.", DateTime.Now);
            logParser.Push("A froglok dar knight hits YOU for 95 points of damage.", DateTime.Now);
            logParser.Push("A froglok dar knight hits YOU for 17 points of damage.", DateTime.Now.AddSeconds(2));
            logParser.Push("You regain your concentration and continue your casting.", DateTime.Now.AddSeconds(2));
            logParser.Push("A froglok dar knight's feet adhere to the ground.", DateTime.Now.AddSeconds(2));

            var spellEffect = SpellList.FirstOrDefault(a=> a.Id == "Paralyzing Earth") as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.HasCount(1, SpellList);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual(" A froglok dar knight", spellEffect.Target);
        }

        [TestMethod]
        public void GuessDruidThorns()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Druid;
            player.Player.Level = 60;
            logParser.Push("You begin casting Legacy of Thorn.", DateTime.Now);
            logParser.Push("Someone is surrounded by a thorny barrier.", DateTime.Now.AddSeconds(3));

            var spellEffect = SpellList.FirstOrDefault() as SpellViewModel;
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual("Legacy of Thorn", spellEffect.Id);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffect.Caster);
            Assert.AreEqual("Someone", spellEffect.Target);
        }

        [TestMethod]
        public void CloudOfSilence()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Druid;
            player.Player.Level = 60;
            player.Player.Zone = "growthplane";
            logParser.Push("You are in a cloud of silence.", DateTime.Now);

            var spellEffect = TimerList.FirstOrDefault();
            Assert.IsNotNull(spellEffect);
            Assert.AreEqual(CustomTimer.CustomerTime, spellEffect.Target);
            Assert.AreEqual("Cloud of Silence", spellEffect.Id);
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
            spellWindowViewModel.UpdateTriggers(1000);
            
            var spellEffects = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Wake of Tranquility").Cast<SpellViewModel>();
            // TODO: Only one of these is being tagged properly. Ideally every single one would be tagged as " You " for the caster
            // TODO: If we ever fix that, update this test to be an .All instead of .Any
            Assert.IsTrue(spellEffects.Any(x => x.Caster == EQSpells.SpaceYou));
            Assert.Contains(" A wooly mammoth", spellEffects.Select(x => x.Target));
            Assert.Contains(" A tundra mammoth", spellEffects.Select(x => x.Target));
            Assert.HasCount(2, spellEffects);
        }

        [TestMethod]
        public void TestRampage()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 60;
            var d = DateTime.Now;
            logParser.Push(spellWindowViewModel, "You feel the urge to rampage.", d);
            spellWindowViewModel.UpdateTriggers(1000);
            
            var spellEffects = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Rampage").Cast<SpellViewModel>();
            Assert.HasCount(1, spellEffects);
        }

        [TestMethod]
        public void TestVampiricEmbrace()
        {
            player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            player.Player.Level = 60;
            var d = DateTime.Now;
            logParser.Push(spellWindowViewModel, "You begin casting Vampiric Embrace.", d);
            logParser.Push(spellWindowViewModel, "Your hand begins to glow.", d.AddSeconds(4));
            spellWindowViewModel.UpdateTriggers(1000);
            
            var spellEffects = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Vampiric Embrace").Cast<SpellViewModel>();
            Assert.HasCount(1, spellEffects);
            Assert.AreEqual(EQSpells.SpaceYou, spellEffects.First().Caster);
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
            spellWindowViewModel.UpdateTriggers(1000);
            
            var spellEffects = spellWindowViewModel.SpellList.Where(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Id == "Clinging Darkness").Cast<SpellViewModel>();
            Assert.HasCount(1, spellEffects);
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

            Assert.HasCount(2, SpellList);
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
            
            Assert.HasCount(1, TimerList);
            
            logParser.Push("A Dizok Underling hits a Dizok Observer for 37 points of damage.", DateTime.Now);
            logParser.Push("A Dizok Underling hits a Dizok Observer for 121 points of damage.", DateTime.Now);
            logParser.Push("a Dizok Observer has been slain by a Dizok Underling!", DateTime.Now);
            logParser.Push("Your faction standing with GoblinsofMountainDeath got better.", DateTime.Now);
            logParser.Push("Your faction standing with SarnakCollective could not possibly get any worse.", DateTime.Now);
            logParser.Push("You gain party experience!!", DateTime.Now);
            logParser.Push("a Dizok Observer begins to shake as the mana burns within its body.", DateTime.Now);
            
            Assert.HasCount(2, TimerList);
        }
    }
}
