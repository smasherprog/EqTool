using Autofac;
using EQToolShared.Discord;
using EQToolShared.Enums;
using EQToolTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class AuctionParsingTests
    {
        private readonly IContainer container;
        private readonly DiscordAuctionParse discordAuctionParse;
        public AuctionParsingTests()
        {
            container = DI.Init();
            discordAuctionParse = container.Resolve<DiscordAuctionParse>();
        }

        [TestMethod]
        public void Parse1()
        {
            var result = discordAuctionParse.Parse("Mulzmulz auctions, 'Wts Djinni Blood'");
            Assert.AreEqual("Mulzmulz", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Djinni Blood", result.Items[0].Name);
            Assert.IsNull(result.Items[0].Price);
        }

        [TestMethod]
        public void Parse2()
        {
            var result = discordAuctionParse.Parse("Mulzmulz auctions, 'Djinni Blood'");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Parse3()
        {
            var result = discordAuctionParse.Parse("Fpwar auctions, 'WTB Nathsar Bracer 200pp/Nathsar Helm 400pp'");
            Assert.AreEqual("Fpwar", result.Player);
            Assert.AreEqual(AuctionType.WTB, result.Items[0].AuctionType);
            Assert.AreEqual("Nathsar Bracer", result.Items[0].Name);
            Assert.AreEqual(200, result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTB, result.Items[1].AuctionType);
            Assert.AreEqual("Nathsar Helm", result.Items[1].Name);
            Assert.AreEqual(400, result.Items[0].Price);
        }

        [TestMethod]
        public void Parse4()
        {
            var result = discordAuctionParse.Parse("Darkinvader auctions, 'WTS Skyfury Scimitar / Othmir Fur x15 / Black Ice Leggings / Spell: Focus of Spirit / Spell: Death Pact / Crustacean Shell Gauntlets / Crustacean Shell Shield'");
            Assert.AreEqual("Darkinvader", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Skyfury Scimitar", result.Items[0].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Othmir Fur", result.Items[1].Name);
            Assert.IsNull(result.Items[1].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[2].AuctionType);
            Assert.AreEqual("Spell: Focus of Spirit", result.Items[2].Name);
            Assert.IsNull(result.Items[2].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[2].AuctionType);
            Assert.AreEqual("Crustacean Shell Shield", result.Items[6].Name);
            Assert.IsNull(result.Items[6].Price);
        }
    }
}
