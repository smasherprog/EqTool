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
            var spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == EQTool.ViewModels.SpellWindow.SpellViewModelType.Spell && a.Name == "Peggy Levitate");
            Assert.IsNotNull(spell);
        }

    }
}
