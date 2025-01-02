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
            var d = DateTime.Now;
            logParser.Push("Brenn slices Bristlebane Puppet for 56 points of damage.", d);
            d = d.AddSeconds(1);
            logParser.Push("You slash Bristlebane Puppet for 31 points of damage.", d);
            logParser.Push("Your target resisted the LowerElement spell.", d);
            var spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "LowerElement") as CounterViewModel;
            Assert.AreEqual(spell.Count, 1);
            d = d.AddSeconds(1);
            logParser.Push("Your target resisted the LowerElement spell.", d);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "LowerElement") as CounterViewModel;
            Assert.AreEqual(spell.Count, 2);
            d = d.AddSeconds(1);
            logParser.Push("Your target resisted the Rage of Vallon spell.", d);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "Rage of Vallon") as CounterViewModel;
            Assert.AreEqual(spell.Count, 1);
            d = d.AddSeconds(1);
            logParser.Push("Bristlebane Puppet is weakened by the Rage of Vallon.", d);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "Rage of Vallon") as CounterViewModel;
            Assert.AreEqual(spell.Count, 2)
                ; d = d.AddSeconds(1);
            logParser.Push("Your target resisted the Rage of Vallon spell.", d);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "Rage of Vallon") as CounterViewModel;
            Assert.AreEqual(spell.Count, 3);
            d = d.AddSeconds(1);
            logParser.Push("Your Rage of Vallon spell has worn off.", d);
            logParser.Push("Your target resisted the Rage of Vallon spell.", d);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "Rage of Vallon") as CounterViewModel;
            Assert.AreEqual(spell.Count, 4);

            d = d.AddSeconds(1);
            logParser.Push("Bristlebane Puppet was tormented.", d);
            logParser.Push("Bristlebane Puppet was hit by non-melee for 10 points of damage.", d);
            logParser.Push("Bristlebane Puppet hits YOU for 150 points of damage.", d);

            d = d.AddSeconds(1);
            logParser.Push("Your target resisted the Rage of Vallon spell.", d);
            spell = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Counter && a.Name == "Rage of Vallon") as CounterViewModel;
            Assert.AreEqual(spell.Count, 5);

        }
    }
}
