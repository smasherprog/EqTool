using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

        //this has a message that it has landed
        [TestMethod]
        public void NecroDA1()
        {
            logParser.Push("You are surrounded by the Quivering Veil of Xarn.", DateTime.Now);
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Quivering Veil of Xarn") as TimerViewModel;
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
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Harmshield") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 599.0, 2);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Harmshield") as SpellViewModel;
            spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Harmshield") as SpellViewModel;
            Assert.AreEqual(spelleffect.TotalDuration.TotalSeconds, 17.0, 2);
        }

        [TestMethod]
        public void Harvest()
        {
            logParser.Push("You gather mana from your surroundings.", DateTime.Now);
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Harvest") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 600.0);
        }

        //this DOES NOT HAVE a landed message
        [TestMethod]
        public void ClericDA1()
        {
            logParser.Push(YouBeginCasting + " Divine Aura.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(3));
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Divine Aura") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 899.0, 2);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Divine Aura") as SpellViewModel;
            Assert.AreEqual(spelleffect.TotalDuration.TotalSeconds, 17.0, 2);
        }

        [TestMethod]
        public void ClericDA2()
        {
            logParser.Push("You are surrounded by a divine barrier.", DateTime.Now);
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Divine Barrier") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 900.0);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Divine Barrier") as SpellViewModel;
            Assert.AreEqual(spelleffect.TotalDuration.TotalSeconds, 18.0);
        }

        [TestMethod]
        public void Dictate()
        {
            logParser.Push("You begin casting Dictate.", DateTime.Now);
            logParser.Push(DummyEntryToForceEmitEvent, DateTime.Now.AddSeconds(7));
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Dictate") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 299.0, 2);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Dictate") as SpellViewModel;
            Assert.AreEqual(spelleffect.TotalDuration.TotalSeconds, 47.0, 2);
        }

        [TestMethod]
        public void TOT()
        {
            logParser.Push("You begin casting Theft of Thought.", DateTime.Now);
            logParser.Push("Someone staggers.", DateTime.Now.AddSeconds(3));
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Theft of Thought") as TimerViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 119, 2);
            Assert.AreEqual(dteffect.GroupName, EQSpells.SpaceYou);
        }
    }
}
