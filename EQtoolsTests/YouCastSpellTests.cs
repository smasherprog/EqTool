using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class YouCastSpellTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells spells;
        private const string YouBeginCasting = "You begin casting ";

        public YouCastSpellTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
            spells = container.Resolve<EQSpells>();
        }

        [TestMethod]
        public void Test1()
        {
            var spellname = "Acumen";
            var spellclass = spells.AllSpells.FirstOrDefault(a => a.name == spellname);
            var line = YouBeginCasting + " " + spellname; 
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Shaman; 
            logParser.Push(line, DateTime.Now);
            line = "Joe " + spellclass.cast_on_other;
            logParser.Push(line, DateTime.Now);

            var effect = spellWindowViewModel.SpellList.FirstOrDefault();
            Assert.IsNotNull(effect); 
        } 
    }
}
