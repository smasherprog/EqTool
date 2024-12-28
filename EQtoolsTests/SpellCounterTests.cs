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
    public class SpellCounterTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public SpellCounterTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
        }

        //this has a message that it has landed
        [TestMethod]
        public void CounterTest1()
        {
            logParser.Push("Vebanab slices a willowisp for 56 points of damage.", DateTime.Now);
            logParser.Push("Your target resisted the LowerElement spell.", DateTime.Now);
            var spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "LowerElement") as CounterViewModel;
            Assert.AreEqual(spell.Count, 1);
            logParser.Push("Your target resisted the LowerElement spell.", DateTime.Now);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "LowerElement") as CounterViewModel;
            Assert.AreEqual(spell.Count, 2);
            logParser.Push("Your target resisted the Rage of Vallon spell.", DateTime.Now);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "Rage of Vallon") as CounterViewModel;
            Assert.AreEqual(spell.Count, 1);
            logParser.Push("a willowisp is weakened by the Rage of Vallon.", DateTime.Now);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "Rage of Vallon") as CounterViewModel;
            Assert.AreEqual(spell.Count, 2);
        }
    }
}
