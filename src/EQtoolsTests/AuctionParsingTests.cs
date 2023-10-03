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
    }
}
