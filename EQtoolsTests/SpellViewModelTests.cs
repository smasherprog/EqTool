using Autofac;
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

        public SpellViewModelTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
        }

        [TestMethod]
        public void NecroDA1()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Quivering Veil of Xarn") as TimerViewModel;
            Assert.IsNull(dteffect);
            logParser.Push("You are surrounded by the Quivering Veil of Xarn.", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Quivering Veil of Xarn") as TimerViewModel;
            Assert.IsNotNull(dteffect);
            Assert.IsTrue(dteffect.TotalDuration.TotalSeconds == 600.0);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Quivering Veil of Xarn") as SpellViewModel;
            Assert.IsNull(spelleffect);
            spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Quivering Veil of Xarn") as SpellViewModel;
            Assert.IsNotNull(spelleffect);
            Assert.IsTrue(spelleffect.TotalDuration.TotalSeconds == 18.0);
        }

        [TestMethod]
        public void NecroDA2()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Harmshield") as TimerViewModel;
            Assert.IsNull(dteffect);
            logParser.Push("You no longer feel pain.", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Harmshield") as TimerViewModel;
            Assert.IsNotNull(dteffect);
            Assert.IsTrue(dteffect.TotalDuration.TotalSeconds == 600.0);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Harmshield") as SpellViewModel;
            Assert.IsNull(spelleffect);
            spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Harmshield") as SpellViewModel;
            Assert.IsNotNull(spelleffect);
            Assert.IsTrue(spelleffect.TotalDuration.TotalSeconds == 18.0);
        }

        [TestMethod]
        public void Harvest()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Harvest") as TimerViewModel;
            Assert.IsNull(dteffect);
            logParser.Push("You gather mana from your surroundings.", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Harvest") as TimerViewModel;
            Assert.IsNotNull(dteffect);
            Assert.IsTrue(dteffect.TotalDuration.TotalSeconds == 600.0);
        }

        [TestMethod]
        public void ClericDA1()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Divine Aura") as TimerViewModel;
            Assert.IsNull(dteffect);
            logParser.Push("The gods have rendered you invulnerable.", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Divine Aura") as TimerViewModel;
            Assert.IsNotNull(dteffect);
            Assert.IsTrue(dteffect.TotalDuration.TotalSeconds == 900.0);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Divine Aura") as SpellViewModel;
            Assert.IsNull(spelleffect);
            spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Divine Aura") as SpellViewModel;
            Assert.IsNotNull(spelleffect);
            Assert.IsTrue(spelleffect.TotalDuration.TotalSeconds == 18.0);
        }

        [TestMethod]
        public void ClericDA2()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Divine Barrier") as TimerViewModel;
            Assert.IsNull(dteffect);
            logParser.Push("You are surrounded by a divine barrier.", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Divine Barrier") as TimerViewModel;
            Assert.IsNotNull(dteffect);
            Assert.IsTrue(dteffect.TotalDuration.TotalSeconds == 900.0);
            var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Divine Barrier") as SpellViewModel;
            Assert.IsNull(spelleffect);
            spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Divine Barrier") as SpellViewModel;
            Assert.IsNotNull(spelleffect);
            Assert.IsTrue(spelleffect.TotalDuration.TotalSeconds == 18.0);
        }

        [TestMethod]
        public void Dictate()
        {
            //var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Dictate") as TimerViewModel;
            //Assert.IsNull(dteffect);
            //logParser.Push("You begin casting Dictate.", DateTime.Now);
            //dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == "Dictate") as TimerViewModel;
            //Assert.IsNotNull(dteffect);
            //Assert.IsTrue(dteffect.TotalDuration.TotalSeconds == 300.0);
            //var spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name == "Dictate") as SpellViewModel;
            //Assert.IsNull(spelleffect);
            //spelleffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Dictate") as SpellViewModel;
            //Assert.IsNotNull(spelleffect);
            //Assert.IsTrue(spelleffect.TotalDuration.TotalSeconds == 6 * 8);
        }
    }
}
