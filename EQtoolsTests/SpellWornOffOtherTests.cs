using EQTool.Services;
using EQTool.ViewModels.SpellWindow;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Autofac;
using EQTool.Services.Parsing;

namespace EQtoolsTests
{
    [TestClass]
    public class SpellWornOffOtherTests : BaseTestClass
    {
        private readonly SpellWornOffOtherParser parser;
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public SpellWornOffOtherTests()
        {
            parser = container.Resolve<SpellWornOffOtherParser>(); 
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
        }

        [TestMethod]
        public void VenomOfTheSnake()
        {
            var now = DateTime.Now;
            var line = "Your Venom of the Snake spell has worn off.";
            var e = parser.MatchWornOffOther(line, now, 0);

            Assert.IsNotNull(e);
            Assert.AreEqual(now, e.TimeStamp);
            Assert.AreEqual(line, e.Line);
            Assert.AreEqual("Venom of the Snake", e.SpellName);
        }

        [TestMethod]
        public void VenomOfTheSnakeViewModel()
        {
            this.player.Player.PlayerClass = EQToolShared.Enums.PlayerClasses.Necromancer;
            this.player.Player.Level = 53;
            logParser.Push("Someone has been poisoned.", DateTime.Now);
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Envenomed Bolt") as SpellViewModel;
            Assert.AreEqual(dteffect.TotalDuration.TotalSeconds, 41, 2);
            logParser.Push("Your Envenomed Bolt spell has worn off.", DateTime.Now.AddSeconds(40));
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Spell && a.Name == "Envenomed Bolt") as SpellViewModel;
            Assert.IsNull(dteffect);
        }

        [TestMethod]
        public void BoilBlood()
        {
            var now = DateTime.Now;
            var line = "Your Boil Blood spell has worn off.";
            var e = parser.MatchWornOffOther(line, now, 0);

            Assert.IsNotNull(e);
            Assert.AreEqual(now, e.TimeStamp);
            Assert.AreEqual(line, e.Line);
            Assert.AreEqual("Boil Blood", e.SpellName);
        }

        [TestMethod]
        public void Fear()
        {
            var now = DateTime.Now;
            var line = "Your Fear spell has worn off.";
            var e = parser.MatchWornOffOther(line, now, 0);

            Assert.IsNotNull(e);
            Assert.AreEqual(now, e.TimeStamp);
            Assert.AreEqual(line, e.Line);
            Assert.AreEqual("Fear", e.SpellName);
        }

    }
}
