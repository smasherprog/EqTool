using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    // Exercises the built-in "Death Touch (Fright/Dread)" trigger that replaced the
    // old DeathTouchHandler. Fright and Dread announce their target by name (a single
    // word) just before death touching it, which starts a 45 second countdown timer.
    [TestClass]
    public class DeathTouchTriggerTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public DeathTouchTriggerTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();

            // The Death Touch built-in only fires in the Plane of Fear, so put the player there.
            player.Player.Zone = "fearplane";

            // Built-in triggers are a read-only library that is disabled by default.
            // Enable the Death Touch trigger so the trigger pipeline evaluates it.
            var settings = container.Resolve<EQToolSettings>();
            var trigger = BuiltInTriggers.CreateDeathTouch();
            trigger.TriggerEnabled = true;
            settings.Triggers.Add(trigger);
        }

        [TestMethod]
        public void SingleWordTargetCreatesTimer()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNull(dteffect);
            logParser.Push("Dread says 'TINIALITA'", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNotNull(dteffect);
            Assert.AreEqual("--DT-- 'TINIALITA'", dteffect.Name);
        }

        [TestMethod]
        public void MultiWordSayIsIgnored()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNull(dteffect);
            logParser.Push("Dread says, 'You will not evade me Silvose!'", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNull(dteffect);
        }

        [TestMethod]
        public void FrightAlsoCreatesTimer()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNull(dteffect);
            logParser.Push("Fright says 'TINIALITA'", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNotNull(dteffect);
        }
    }
}
