using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using EQtoolsTests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class SpellAutomaticGroupingTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly EQSpells eqSpells;
        private readonly EQToolSettings settings;

        public SpellAutomaticGroupingTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
            eqSpells = container.Resolve<EQSpells>();
            settings = container.Resolve<EQToolSettings>();

            spellWindowViewModel.groupRevaluationDebounceTime = 25;    // Don't want the default value for tests, we just need something semi-noticeable 
            settings._SpellsFilter = SpellsFilterType.ByClass;
        }

        [TestMethod]
        public void AutomaticGrouping_SharedSpell_CoupleOfNPCs_DoesNotCreateOrphans()
        {
            // Arrange
            settings.NpcSpellGroupingType = SpellGroupingType.Automatic;

            // Act
            var shouldBeByTarget = new List<(string SpellName, string Target)>();
            PushAuthenticSpellCast(shouldBeByTarget, "Fetter", "a geonid", EQSpells.You);
            PushAuthenticSpellCast(shouldBeByTarget, "Tashanian", "a geonid", EQSpells.You);
            PushAuthenticSpellCast(shouldBeByTarget, "Tashanian", "a geonid", EQSpells.You);
            PushAuthenticSpellCast(shouldBeByTarget, "Tashanian", "a geonid shaman", EQSpells.You);
            PushAuthenticSpellCast(shouldBeByTarget, "Pacify", "a geonid shaman", EQSpells.You);
            SleepToLetHandlersProcess();
            
            // Assert
            // Despite 3 instances of Tash, we don't want to group anything by id because it would leave the npcs orphaned and create 3 distinct groupings
            AssertSpellListMatchesExpectations(shouldBeByTarget, new List<(string, string)>());
        }

        [TestMethod]
        public void AutomaticGrouping_ManyGroupBuffs_WithSomeOneOffs_PerformsAsExpected()
        {
            // Arrange
            settings.PlayerSpellGroupingType = SpellGroupingType.Automatic;

            var groupBuffTargets = new List<string> { EQSpells.You, "Joe", "Huntor", "Sanare", "Pigy" };
            var oneOffTargets = new List<string> {"Aasgard", "Pigy"};

            // Act
            var shouldBeByTarget = new List<(string SpellName, string Target)>();
            AddDefaultSetOfSelfCastOneOffSpells(shouldBeByTarget);
            
            var shouldBeById = new List<(string SpellName, string Target)>();
            PushAuthenticSpellCast(shouldBeById, "Visions of Grandeur", oneOffTargets[0], EQSpells.You);
            PushAuthenticSpellCast(shouldBeById, "Visions of Grandeur", oneOffTargets[1]);
            PushAuthenticAoESpellCast(shouldBeById, "Group Resist Magic", groupBuffTargets, EQSpells.You);
            PushAuthenticAoESpellCast(shouldBeById, "Gift of Pure Thought", groupBuffTargets, EQSpells.You);
            SleepToLetHandlersProcess();
            
            // Assert
            AssertSpellListMatchesExpectations(shouldBeByTarget, shouldBeById);
        }

        // 4 "one-off" spells that should not be grouped by id, all on the player.
        private void AddDefaultSetOfSelfCastOneOffSpells(List<(string SpellName, string Target)> shouldBeByTarget)
        {
            PushAuthenticSpellCast(shouldBeByTarget, "Enlightenment", EQSpells.You, EQSpells.You);
            PushAuthenticSpellCast(shouldBeByTarget, "Overwhelming Splendor", EQSpells.You, EQSpells.You);
            PushAuthenticSpellCast(shouldBeByTarget, "Augment", EQSpells.You, EQSpells.You);
            PushAuthenticSpellCast(shouldBeByTarget, "Gift of Brilliance", EQSpells.You, EQSpells.You);
        }
        
        private void AssertSpellListMatchesExpectations(List<(string SpellName, string Target)> shouldBeByTarget, List<(string SpellName, string Target)> shouldBeById)
        {
            foreach (var spell in spellWindowViewModel.SpellList.OfType<SpellViewModel>())
            {
                if (shouldBeByTarget.Any(expected => LazyMatches(expected, spell)))
                    Assert.IsFalse(spell.IsCategorizeById, $"Spell: \"{spell.Id}\",  Target: \"{spell.Target}\", Should not be categorized by Id, but it was.");
                else if (shouldBeById.Any(expected => LazyMatches(expected, spell)))
                    Assert.IsTrue(spell.IsCategorizeById, $"Spell: \"{spell.Id}\",  Target: \"{spell.Target}\", Should be categorized by Id, but it wasn't.");
                else
                    Assert.Fail($"Faulty Test Data. Expected Combination not found for Spell: \"{spell.Id}\",  Target: \"{spell.Target}\"");
            }
        }

        private static void SleepToLetHandlersProcess() => Thread.Sleep(150); // Let all the handlers and nonsense process
        
        // Lazily just adding an extra check for a " target" in addition to their exact name so we don't have to figure out if they are an npc or a player
        private static bool LazyMatches((string SpellName, string Target) expected, SpellViewModel instance)
            => instance.Id == expected.SpellName && (instance.Target == expected.Target  || instance.Target == " " + expected.Target);

        private DateTime PushAuthenticSpellCast(List<(string SpellName, string Target)> listToAddTo, string spellName, string target, string caster = null, DateTime? dateTime = null)
        {
            var spell = eqSpells.AllSpells[spellName];
            var dt = logParser.PushAuthenticSpellCast(spellWindowViewModel, spell, target, caster, dateTime);
            listToAddTo.Add(target == EQSpells.You ? (SpellName: spell.name, Target: EQSpells.SpaceYou) : (SpellName: spell.name, Target: target));
            
            return dt;
        }

        private DateTime PushAuthenticAoESpellCast(List<(string SpellName, string Target)> listToAddTo, string spellName, List<string> targets, string caster = null, DateTime? dateTime = null)
        {
            var spell = eqSpells.AllSpells[spellName];
            var dt = logParser.PushAuthenticAoESpellCast(spellWindowViewModel, spell, targets, caster, dateTime);
            listToAddTo.AddRange(targets.Select(target => target == EQSpells.You ? (spell.name, EQSpells.SpaceYou) : (spell.name, target)));
            return dt;
        }
    }
}
