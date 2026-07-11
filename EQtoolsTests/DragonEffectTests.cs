using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class DragonEffectTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly EQSpells spells;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly FightHistory fightHistory;

        public DragonEffectTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            fightHistory = container.Resolve<FightHistory>();
            spells = container.Resolve<EQSpells>();
            logEvents = container.Resolve<LogEvents>();
            logParser = container.Resolve<LogParser>();
            player.Player.Zone = string.Empty;
        }

        //[TestMethod]
        //public void TestDragonEffects()
        //{
        //    foreach (var zone in Zones.ZoneInfoMap.Values.Where(a => a.NPCThatAOE.Any()))
        //    {
        //        logParser.Push($"You have entered {zone.Name}", DateTime.Now);
        //        foreach (var npc in zone.NPCThatAOE)
        //        {
        //            fightHistory.clean(DateTime.Now.AddMinutes(15));
        //            spellWindowViewModel.SpellList.Clear();
        //            var message = $"{npc.Name} bashes YOU for 12 points of damage.";
        //            logParser.Push(message, DateTime.Now);
        //            foreach (var spellname in npc.SpellEffects)
        //            {
        //                var spell = spells.AllSpells[spellname];
        //                logParser.Push(spell.cast_on_you, DateTime.Now);
        //                var effect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == spellname) as TimerViewModel;
        //                Assert.IsNotNull(effect, $"{spellname} not found");
        //                Assert.AreEqual(spell.recastTime / 1000, effect.TotalDuration.TotalSeconds);
        //                Assert.AreEqual(spell.name, effect.Name);
        //            }
        //        }
        //    }
        //}

        //[TestMethod]
        //public void TestResistDragonEffects()
        //{
        //    foreach (var zone in Zones.ZoneInfoMap.Values.Where(a => a.NPCThatAOE.Any()))
        //    {
        //        logParser.Push($"You have entered {zone.Name}", DateTime.Now);
        //        foreach (var npc in zone.NPCThatAOE)
        //        {
        //            spellWindowViewModel.SpellList.Clear();
        //            foreach (var spellname in npc.SpellEffects)
        //            {
        //                var spell = spells.AllSpells[spellname];
        //                logParser.Push($"You resist the {spellname} spell!", DateTime.Now);
        //                var effect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Name == spellname) as TimerViewModel;
        //                Assert.IsNotNull(effect, $"{spellname} not found");
        //                Assert.AreEqual(spell.recastTime / 1000, effect.TotalDuration.TotalSeconds);
        //                Assert.AreEqual(spell.name, effect.Name);
        //            }
        //        }
        //    }
        //}
    }
}
