using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class DragonEffectTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly EQSpells spells;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly FightHistory fightHistory;

        public DragonEffectTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            fightHistory = container.Resolve<FightHistory>();
            spells = container.Resolve<EQSpells>();
            logEvents = container.Resolve<LogEvents>();
            logParser = container.Resolve<LogParser>();
            this.player.Player.Zone = string.Empty;
        }
        [TestMethod]
        public void TestRemoteDragonRoarHappyPathNoLoc()
        {
            this.player.Player.Zone = "templeveeshan";
            this.player.Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            var spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);

            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);

            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
            this.logParser.Push("A blast of cold freezes your skin.", DateTime.Now);
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
        }

        [TestMethod]
        public void TestRemoteDragonRoarHappyPathWithLocs()
        {
            this.player.Player.Zone = "templeveeshan";
            this.player.Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            var spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);

            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);

            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(100, 0, 0) });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
            this.logParser.Push("A blast of cold freezes your skin.", DateTime.Now);
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
        }


        [TestMethod]
        public void TestRemoteDragonRoarTestOutOfRange()
        {
            this.player.Player.Zone = "templeveeshan";
            this.player.Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            var spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(0, spellvm.Count);

            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(0, spellvm.Count);

            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(0, spellvm.Count);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(0, spellvm.Count);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(1005, 0, 0) });
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Rain of Molten Lava").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(0, spellvm.Count);
            this.logParser.Push("A blast of cold freezes your skin.", DateTime.Now);
            spellvm = spellWindowViewModel.SpellList.Where(a => a.Name == "Wave of Cold").Cast<TimerViewModel>().ToList();
            Assert.AreEqual(1, spellvm.Count);
        }

        [TestMethod]
        public void TestRemoteDragonRoar()
        {
            this.player.Player.Zone = "templeveeshan";
            this.player.Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0);

            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava", Location = new System.Windows.Media.Media3D.Point3D(0, 0, 0) });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            this.logParser.Push("A blast of cold freezes your skin.", DateTime.Now);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            this.player.Player.Zone = "necropolis";

            this.logParser.Push("You resist the Dragon Roar spell!", DateTime.Now);
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Dragon Roar" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Stun Breath" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rotting Flesh" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Putrefy Flesh" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rain of Molten Lava" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Wave of Cold" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Stun Breath" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Rotting Flesh" });
            this.logEvents.Handle(new DragonRoarRemoteEvent { SpellName = "Putrefy Flesh" });
        }


        [TestMethod]
        public void TestDragonEffects()
        {
            foreach (var zone in Zones.ZoneInfoMap.Values.Where(a => a.NPCThatAOE.Any()))
            {
                logParser.Push($"You have entered {zone.Name}", DateTime.Now);
                foreach (var npc in zone.NPCThatAOE)
                {
                    this.fightHistory.clean(DateTime.Now.AddMinutes(15));
                    spellWindowViewModel.SpellList.Clear();
                    var message = $"{npc.Name} bashes YOU for 12 points of damage.";
                    logParser.Push(message, DateTime.Now);
                    foreach (var spellname in npc.SpellEffects)
                    {
                        var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                        logParser.Push(spell.cast_on_you, DateTime.Now);
                        var effect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == spellname) as TimerViewModel;
                        Assert.IsNotNull(effect, $"{spellname} not found");
                        Assert.AreEqual(effect.TotalDuration.TotalSeconds, spell.recastTime / 1000);
                        Assert.AreEqual(effect.Name, spell.name);
                    }
                }
            }
        }

        [TestMethod]
        public void TestResistDragonEffects()
        {
            foreach (var zone in Zones.ZoneInfoMap.Values.Where(a => a.NPCThatAOE.Any()))
            {
                logParser.Push($"You have entered {zone.Name}", DateTime.Now);
                foreach (var npc in zone.NPCThatAOE)
                {
                    spellWindowViewModel.SpellList.Clear();
                    foreach (var spellname in npc.SpellEffects)
                    {
                        var spell = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
                        logParser.Push($"You resist the {spellname} spell!", DateTime.Now);
                        var effect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == spellname) as TimerViewModel;
                        Assert.IsNotNull(effect, $"{spellname} not found");
                        Assert.AreEqual(effect.TotalDuration.TotalSeconds, spell.recastTime / 1000);
                        Assert.AreEqual(effect.Name, spell.name);
                    }
                }
            }
        }
    }
}
