using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQToolTests
{
    [TestClass]
    public class EnrageTests
    {
        private readonly IContainer container;
        public EnrageTests()
        {
            container = DI.Init();
        }

        [TestMethod]
        public void Parse1()
        {
            var service = container.Resolve<EnrageParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                Zone = "templeveeshan"
            };

            var d = service.EnrageCheck("Cekenar has become ENRAGED.");
            Assert.AreEqual(d.NpcName, "Cekenar");
        }

        [TestMethod]
        public void ParseNotInzone()
        {
            var service = container.Resolve<EnrageParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                Zone = "swampofnohope"
            };

            var d = service.EnrageCheck("Cekenar has become ENRAGED.");
            Assert.IsNull(d);
        }
    }
}
