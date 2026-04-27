using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class SpellAdaptiveGroupingTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells eqSpells;
        private readonly EQToolSettings settings;

        public SpellAdaptiveGroupingTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
            eqSpells = container.Resolve<EQSpells>();
            settings = container.Resolve<EQToolSettings>();

            player.Player.TimerRecastSetting = TimerRecast.StartNewTimer;
            player.Player.Level = 60;
            player.Player.PlayerClass = PlayerClasses.Enchanter;  // Don't want the default value for tests, we just need something semi-noticeable 
        }

        [TestMethod]
        public void RoutineDebuffScenario1_AllAreGroupedByTarget()
        {
            var spell = this.eqSpells.AllSpells["Tepid Deeds"];

            logParser.Push($"A Dizok Underling {spell.cast_on_other}", DateTime.Now);
        }

        [TestMethod]
        public void BasicGroupBuffScenario1_AllById()
        {
        }

        [TestMethod]
        public void DuoBuffScenario1_AllByTarget()
        {

        }

        [TestMethod]
        public void AllGroupedById_SomeSpellsFallOff_OrphanIsNowGroupedByTarget()
        {
        }

        [TestMethod]
        public void ComplexBuffScenario1_PerformsAsExpected()
        {
        }

        [TestMethod]
        public void ComplexBuffScenario2_PerformsAsExpected()
        {
        }
    }
}
