using EQTool.Models;
using EQTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class TriggerTestSampleGeneratorTests
    {
        [TestMethod]
        public void EveryBuiltInTriggerGeneratesAMatchingSample()
        {
            var failures = new List<string>();
            foreach (var trigger in BuiltInTriggers.All())
            {
                trigger.PlayerName = "Soandso";
                var sample = TriggerTestSampleGenerator.Generate(trigger, "Soandso");
                if (string.IsNullOrEmpty(sample) || !trigger.Matches(sample))
                {
                    failures.Add($"{trigger.TriggerName} [{trigger.SearchText}] -> \"{sample}\"");
                }
            }

            Assert.AreEqual(0, failures.Count,
                "These built-in triggers produced a non-matching sample:\n" + string.Join("\n", failures));
        }

        [TestMethod]
        public void PlainTextTriggerReturnsSearchTextVerbatim()
        {
            var trigger = new Trigger { SearchText = "Charm has worn off.", UseRegex = false };
            var sample = TriggerTestSampleGenerator.Generate(trigger, null);
            Assert.AreEqual("Charm has worn off.", sample);
            Assert.IsTrue(trigger.Matches(sample));
        }
    }
}
