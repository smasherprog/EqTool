using Autofac;
using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class FTETests : BaseTestClass
    {
        public FTETests()
        {
        }

        [TestMethod]
        public void Parse1()
        {
            var service = container.Resolve<FTEParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                Zone = "templeveeshan"
            };

            var d = service.Parse("Cekenar engages Tzvia!", DateTime.Now, 0);
            Assert.AreEqual(d.FTEPerson, "Tzvia");
            Assert.AreEqual(d.NPCName, "Cekenar");
        }

        [TestMethod]
        public void Prase2()
        {
            var service = container.Resolve<FTEParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                Zone = "templeveeshan"
            };

            var d = service.Parse("Dagarn the Destroyer engages Tzvia!", DateTime.Now, 0);
            Assert.AreEqual(d.FTEPerson, "Tzvia");
            Assert.AreEqual(d.NPCName, "Dagarn the Destroyer");
        }
    }
}
