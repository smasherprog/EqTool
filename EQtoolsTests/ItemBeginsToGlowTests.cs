using Autofac;
using EQTool.Services;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class ItemBeginsToGlowTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public ItemBeginsToGlowTests()
        {
            logParser = container.Resolve<LogParser>();
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
        }

        [TestMethod]
        public void TestPeggyCloak()
        {
            logParser.Push("You begin casting Levitate.", DateTime.Now);
            logParser.Push("Your Pegasus Feather Cloak begins to glow.", DateTime.Now);
            logParser.Push("Your feet leave the ground.", DateTime.Now.AddSeconds(6));
            var spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == EQTool.ViewModels.SpellWindow.SpellViewModelType.Spell && a.Id == "Peggy Levitate");
            Assert.IsNotNull(spell);
        }

        [TestMethod]
        public void NecroStaff()
        {
            logParser.Push("You begin casting Soul Well.", DateTime.Now);
            logParser.Push("Your Shissar Seance Staff begins to glow.", DateTime.Now);
            logParser.Push("You feel your life force drain away.", DateTime.Now.AddSeconds(14));
            logParser.Push("A sepulcher skeleton staggers.", DateTime.Now.AddSeconds(14));
            var spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == EQTool.ViewModels.SpellWindow.SpellViewModelType.Spell && a.Id == "Soul Well");
            Assert.IsNotNull(spell);
        }
    }
}
