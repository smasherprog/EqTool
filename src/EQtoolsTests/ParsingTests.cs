using Autofac;
using EQTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class ParsingTests
    {
        private readonly IContainer container;
        public ParsingTests()
        {
            container = DI.Init();
        }

        private string AddDateTime(DateTime d, string msg)
        {
            var format = "ddd MMM dd HH:mm:ss yyyy";
            return "[" + d.ToString(format) + "] " + msg;
        }

        [TestMethod]
        public void ParseEnterWorld()
        {
            var enterworldparser = container.Resolve<EnterWorldParser>();
            Assert.IsFalse(enterworldparser.HasEnteredWorld("take about 5 more seconds to prepare your camp."));
            Assert.IsFalse(enterworldparser.HasEnteredWorld("45 2023] Welcome to EverQuest!"));
            Assert.IsFalse(enterworldparser.HasEnteredWorld(AddDateTime(DateTime.Now.AddSeconds(-1), "You have entered Plane of Mischief.")));
            Assert.IsTrue(enterworldparser.HasEnteredWorld(AddDateTime(DateTime.Now.AddSeconds(-1), "Welcome to EverQuest!")));
            Assert.IsFalse(enterworldparser.HasEnteredWorld(AddDateTime(DateTime.Now.AddSeconds(-5), "Welcome to EverQuest!")));
        }

        private const string ResponseFromServer = @"{{Disambig3|[[Orc Legionnaire (Crushbone)]] (Faydwer Version)}}
{{Disambig3|[[Orc Legionnaire (Deathfist)]] (Antonica Version)}}";

        [TestMethod]
        public void ParseOrcCenturion()
        {
            var result = WikiApi.Disambig(ResponseFromServer, "Crushbone");
            Assert.AreEqual("Orc Legionnaire (Crushbone)", result);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        [TestMethod]
        public void ParseOrcCenturion1()
        {
            var result = WikiApi.Disambig(ResponseFromServer, "crushbone");
            Assert.AreEqual("Orc Legionnaire (Crushbone)", result);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        [TestMethod]
        public void ParseOrcCenturion_nozone()
        {
            var result = WikiApi.Disambig(ResponseFromServer, null);
            Assert.AreEqual("Orc Legionnaire (Crushbone)", result);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        [TestMethod]
        public void ParseOrcCenturion_oasis()
        {
            var result = WikiApi.Disambig(ResponseFromServer, "oasis");
            Assert.AreEqual("Orc Legionnaire (Deathfist)", result);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        private const string ResponseFromServer1 = @"{{Classic Era}}
{{Disambig3|[[an ancient cyclops (Ocean of Tears)]]}}
{{Disambig3|[[an ancient cyclops (Southern Ro)]]}}

[[File:npc_ancient_cyclops.png]]

The frequently hunted Ancient Cyclops appears in four different zones, although his incarnations in the Karana plains are ''far'' rarer.  All versions drop the {{:Ring of the Ancients}} for the [[Journeyman's Boots Quest]].";

        [TestMethod]
        public void ParseAnAncientCycplosoot()
        {
            var result = WikiApi.Disambig(ResponseFromServer1, "oot");
            Assert.AreEqual("an ancient cyclops (Ocean of Tears)", result);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        [TestMethod]
        public void ParseAnAncientCycplossro()
        {
            var result = WikiApi.Disambig(ResponseFromServer1, "sro");
            Assert.AreEqual("an ancient cyclops (Southern Ro)", result);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

        private const string ResponseFromServer2 = @"{{Disambig3|[[Languages|Lizardman, the language of the Iksar race]]}}
__NOTOC__
[[File:npc_a_lizard_crusader.png|right|250px]]
== Description ==
This race of diminutive humanoids have green, scaley skin and speak in lizard noises. They also have long tongues and a distaste for any outsiders who would defile their temples. They seem to worship the God of Fear, [[Cazic-Thule]].

Lizardmen should never be confused with [[Iksar]]s, who are more properly '''man-lizards'''.

== Lore ==

* Lore Needed

== Locations ==

* [[Cazic Thule]]

* [[The Feerrott]]

== See Also ==

* [[Iksar]]

[[Category: Monster Race]]";

        [TestMethod]
        public void ParseAnDisambig()
        {
            var result = WikiApi.Disambig(ResponseFromServer2, "oot");
            Assert.AreEqual("Languages|Lizardman, the language of the Iksar race", result);
            // Assert.AreEqual("[[Lava Breath]], Enrage, Summon, Uncharmable, Unfearable, Unmezzable, See Invis", model.Special);
        }

    }
}
