using Autofac;
using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQToolTests
{
    [TestClass]
    public class FTETests
    {
        private readonly IContainer container;
        public FTETests()
        {
            container = DI.Init();
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

            var d = service.Parse("Cekenar engages Tzvia!");
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

            var d = service.Parse("Dagarn the Destroyer engages Tzvia!");
            Assert.AreEqual(d.FTEPerson, "Tzvia");
            Assert.AreEqual(d.NPCName, "Dagarn the Destroyer");
        }
    }
}
