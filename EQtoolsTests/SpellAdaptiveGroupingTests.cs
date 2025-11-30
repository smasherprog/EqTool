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
            player.Player.PlayerClass = PlayerClasses.Enchanter;
            settings._SpellsFilter = SpellsFilterType.ByClass;
            spellWindowViewModel.groupRevaluationDebounceTime = 50;    // Don't want the default value for tests, we just need something semi-noticeable 
        }

        [TestMethod]
        public void RoutineDebuffScenario1_AllAreGroupedByTarget()
        {
            /* 2 orphan spells on 2 different targets | Expected: Group By Target
             * 3 instances of Tash on those same 2 targets | Expected: Group By Target
             * Don't group anything by Id because it would leave the npcs with single orphaned spells and result in 3 distinct groupings */
            
            // Arrange
            settings.NpcSpellGroupingType = SpellGroupingType.Adaptive;
            settings.PlayerSpellGroupingType = SpellGroupingType.ByTarget;
            var logTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));

            // Act
            var expectations = new List<Expectation>();
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Tepid Deeds", "a geonid", EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Tashanian", "a geonid", EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Tashanian", "a geonid", EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Tashanian", "a geonid shaman", EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Dazzle", "a geonid shaman", EQSpells.You, logTime);
            
            logParser.UpdateTriggers(spellWindowViewModel, logTime);
            SleepToLetHandlersProcess();
            
            // Assert
            AssertSpellListMatchesExpectations(expectations);
        }

        [TestMethod]
        public void BasicGroupBuffScenario1_AllById()
        {
            /* 2 group buff spells cast on 5 players, all cast by your class | Expected: Group By Id
             * No orphaned spells of any kind. */
            
            // Arrange
            settings.NpcSpellGroupingType = SpellGroupingType.ByTarget;
            settings.PlayerSpellGroupingType = SpellGroupingType.Adaptive;
            
            var groupBuffTargets = new List<string> { EQSpells.You, "Joe", "Huntor", "Sanare", "Pigy" };
            var logTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));

            // Act
            var expectations = new List<Expectation>();
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ById, "Group Resist Magic", groupBuffTargets, EQSpells.You, logTime);
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ById, "Gift of Pure Thought", groupBuffTargets, EQSpells.You, logTime);
            
            logParser.UpdateTriggers(spellWindowViewModel, logTime);
            SleepToLetHandlersProcess();
            
            // Assert
            AssertSpellListMatchesExpectations(expectations);
        }
        
        [TestMethod]
        public void DuoBuffScenario1_AllByTarget()
        {
            /* 4 orphan spells on you | Expected: Group By Target
             * 1 orphan spell on one player | Expected: Group by Target
             * 2 buff spells cast on 2 players, all cast by your class | Expected: Group By Target */
            
            // Arrange
            settings.NpcSpellGroupingType = SpellGroupingType.ByTarget;
            settings.PlayerSpellGroupingType = SpellGroupingType.Adaptive;
            
            var groupBuffTargets = new List<string> { EQSpells.You, "Joe" };
            var logTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));

            // Act
            var expectations = new List<Expectation>();
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Enlightenment", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Overwhelming Splendor", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Augment", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Gift of Brilliance", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Visions of Grandeur", "Aasgard", EQSpells.You, logTime);
            
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ByTarget, "Group Resist Magic", groupBuffTargets, EQSpells.You, logTime);
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ByTarget, "Gift of Pure Thought", groupBuffTargets, EQSpells.You, logTime);
            
            logParser.UpdateTriggers(spellWindowViewModel, logTime);
            SleepToLetHandlersProcess();
            
            // Assert
            AssertSpellListMatchesExpectations(expectations);
        }
                        
        [TestMethod]
        public void AllGroupedById_SomeSpellsFallOff_OrphanIsNowGroupedByTarget()
        {
            /* 2 instances of VoG (class spell) on 2 different players | Expected: Group By Id
             * 2 of your own group buffs on you, with those same buffs on 4 others | Expected: Group by Target
             * 1 existing instance of VoG falls off | Expected: Now Group by Target */
            
            // Arrange
            settings.NpcSpellGroupingType = SpellGroupingType.ByTarget;
            settings.PlayerSpellGroupingType = SpellGroupingType.Adaptive;

            var groupBuffTargets = new List<string> { EQSpells.You, "Joe", "Huntor", "Sanare", "Pigy" };
            var logTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));
            
            var expectations = new List<Expectation>();
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ById, "Group Resist Magic", groupBuffTargets, EQSpells.You, logTime);
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ById, "Gift of Pure Thought", groupBuffTargets, EQSpells.You, logTime);
            
            // Should be ById first, until we remove the one from Aasgard
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Visions of Grandeur", "Pigy", EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast("Visions of Grandeur", "Aasgard", caster: null, logTime);
            
            logParser.UpdateTriggers(spellWindowViewModel, logTime);
            SleepToLetHandlersProcess();

            // Act
            var toRemove = spellWindowViewModel.SpellList.FirstOrDefault(x => x.Id == "Visions of Grandeur" && x.Target == "Aasgard");
            spellWindowViewModel.SpellList.Remove(toRemove);
            SleepToLetHandlersProcess();
            
            // Assert
            AssertSpellListMatchesExpectations(expectations);
        }
        
        [TestMethod]
        public void ComplexBuffScenario1_PerformsAsExpected()
        {
            /* 4 orphan spells on you | Expected: Group By Target
             * 1 instance of VoG (class spell) on 1 player | Expected: Group By Target
             * 2 of your own group buffs on you, with those same buffs on 4 others | Expected: Group By Id */
            
            // Arrange
            settings.NpcSpellGroupingType = SpellGroupingType.ByTarget;
            settings.PlayerSpellGroupingType = SpellGroupingType.Adaptive;

            var groupBuffTargets = new List<string> { EQSpells.You, "Joe", "Huntor", "Sanare", "Pigy" };
            var logTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));

            // Act
            var expectations = new List<Expectation>();
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Enlightenment", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Overwhelming Splendor", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Augment", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Gift of Brilliance", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Visions of Grandeur", "Aasgard", EQSpells.You, logTime);
            
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ById, "Group Resist Magic", groupBuffTargets, EQSpells.You, logTime);
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ById, "Gift of Pure Thought", groupBuffTargets, EQSpells.You, logTime);
            
            logParser.UpdateTriggers(spellWindowViewModel, logTime);
            SleepToLetHandlersProcess();
            
            // Assert
            AssertSpellListMatchesExpectations(expectations);
        }
        
        [TestMethod]
        public void ComplexBuffScenario2_PerformsAsExpected()
        {
            /* 4 orphan spells on you | Expected: Group by target
             * 2 instances of VoG (class spell) on 2 different players | Expected: Group by Id
             * 2 of your own group buffs on you, with those same buffs on 4 others | Expected: Group by Id */
            
            // Arrange
            settings.NpcSpellGroupingType = SpellGroupingType.ByTarget;
            settings.PlayerSpellGroupingType = SpellGroupingType.Adaptive;

            var groupBuffTargets = new List<string> { EQSpells.You, "Joe", "Huntor", "Sanare", "Pigy" };
            var oneOffTargets = new List<string> {"Aasgard", "Pigy"};
            var logTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(1));

            // Act
            var expectations = new List<Expectation>();
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Enlightenment", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Overwhelming Splendor", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Augment", EQSpells.You, EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ByTarget, "Gift of Brilliance", EQSpells.You, EQSpells.You, logTime);

            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ById, "Visions of Grandeur", oneOffTargets[0], EQSpells.You, logTime);
            logTime = PushAuthenticSpellCast(expectations, ExpectedGrouping.ById, "Visions of Grandeur", oneOffTargets[1], caster: null, logTime);
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ById, "Group Resist Magic", groupBuffTargets, EQSpells.You, logTime);
            logTime = PushAuthenticAoESpellCast(expectations, ExpectedGrouping.ById, "Gift of Pure Thought", groupBuffTargets, EQSpells.You, logTime);
            
            logParser.UpdateTriggers(spellWindowViewModel, logTime);
            SleepToLetHandlersProcess();
            
            // Assert
            AssertSpellListMatchesExpectations(expectations);
        }
        
        private void AssertSpellListMatchesExpectations(List<Expectation> expectations)
        {
            var expectedSpellCount = expectations.Count;
            var spellsInSpellList = spellWindowViewModel.SpellList.OfType<SpellViewModel>();
            Assert.AreEqual(expectedSpellCount, spellsInSpellList.Count(), "Faulty Test Setup. The number of expectations does not match the number of active timers.");
            
            foreach (var spell in spellsInSpellList)
            {
                var expectation = expectations.FirstOrDefault(x => LazyMatches(x, spell));
                if (expectation == null)
                    Assert.Fail($"Faulty Test Data. Expected Combination not found for Spell: \"{spell.Id}\",  Target: \"{spell.Target}\"");

                if (expectation.Grouping == ExpectedGrouping.ById)
                    Assert.IsTrue(spell.IsCategorizeById, $"Spell: \"{spell.Id}\",  Target: \"{spell.Target}\", Should be categorized by Id, but it wasn't.");
                else
                    Assert.IsFalse(spell.IsCategorizeById, $"Spell: \"{spell.Id}\",  Target: \"{spell.Target}\", Should not be categorized by Id, but it was.");
            }
        }

        // Let all the handlers and debounces process.
        // I don't love doing this but there's not really a clean way to bypass the debounce without redesigning some stuff or overhauling how we're testing this
        private static void SleepToLetHandlersProcess() => Thread.Sleep(150);
        
        // Lazily just adding an extra check for a " target" in addition to their exact name so we don't have to figure out if they are an npc or a player
        private static bool LazyMatches(Expectation expectation, SpellViewModel instance)
            => instance.Id == expectation.SpellName && (instance.Target == expectation.Target  || instance.Target == " " + expectation.Target);

        private DateTime PushAuthenticSpellCast(string spellName, string target, string caster = null, DateTime? dateTime = null)
        {
            var spell = eqSpells.AllSpells[spellName];
            var dt = logParser.PushAuthenticSpellCast(spell, target, caster: caster, dateTime);
            
            return dt;
        }
        
        private DateTime PushAuthenticSpellCast(List<Expectation> expectations, ExpectedGrouping grouping, string spellName, string target, string caster = null, DateTime? dateTime = null)
        {
            var spell = eqSpells.AllSpells[spellName];
            var dt = logParser.PushAuthenticSpellCast(spell, target, caster: caster, dateTime);
            expectations.Add(new Expectation(spell.name, target == EQSpells.You ? EQSpells.SpaceYou : target, grouping));
            
            return dt;
        }

        private DateTime PushAuthenticAoESpellCast(List<Expectation> expectations, ExpectedGrouping grouping, string spellName, List<string> targets, string caster = null, DateTime? dateTime = null)
        {
            var spell = eqSpells.AllSpells[spellName];
            var dt = logParser.PushAuthenticAoESpellCast(spell, targets, caster: caster, dateTime);
            expectations.AddRange(targets.Select(target => new Expectation(spell.name, target == EQSpells.You ? EQSpells.SpaceYou : target, grouping)));
            
            return dt;
        }

        private enum ExpectedGrouping
        {
            ByTarget,
            ById
        }

        private class Expectation
        {
            public string SpellName { get; }
            public string Target { get; }
            public ExpectedGrouping Grouping { get; }

            public Expectation(string spellName, string target, ExpectedGrouping grouping)
            {
                SpellName = spellName;
                Target = target;
                Grouping = grouping;
            }
        }
    }
}
