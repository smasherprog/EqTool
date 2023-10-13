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
            Assert.AreEqual(400, result.Items[1].Price);
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
            Assert.AreEqual("Black Ice Leggings", result.Items[2].Name);
            Assert.IsNull(result.Items[2].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[3].AuctionType);
            Assert.AreEqual("Spell: Focus of Spirit", result.Items[3].Name);
            Assert.IsNull(result.Items[3].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[6].AuctionType);
            Assert.AreEqual("Crustacean Shell Shield", result.Items[6].Name);
            Assert.IsNull(result.Items[6].Price);
        }

        [TestMethod]
        public void Parse5()
        {
            var result = discordAuctionParse.Parse("Karlsmule auctions, 'WTS Ceremonial Iksar Chestplate 10k | Brown Chitin Protector 150p'");
            Assert.AreEqual("Karlsmule", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Ceremonial Iksar Chestplate", result.Items[0].Name);
            Assert.AreEqual(10000, result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Brown Chitin Protector", result.Items[1].Name);
            Assert.AreEqual(150, result.Items[1].Price);
        }

        [TestMethod]
        public void Parse6()
        {
            var result = discordAuctionParse.Parse("Karlsmule auctions, 'WTS Ceremonial Iksar Chestplate 10k | Brown Chitin Protector 150p'");
            Assert.AreEqual("Karlsmule", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Ceremonial Iksar Chestplate", result.Items[0].Name);
            Assert.AreEqual(10000, result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Brown Chitin Protector", result.Items[1].Name);
            Assert.AreEqual(150, result.Items[1].Price);
        }

        [TestMethod]
        public void Parse7()
        {
            var result = discordAuctionParse.Parse("Bazar auctions, 'WTB Withered Leather Leggings 2k WTB PST'");
            Assert.AreEqual("Bazar", result.Player);
            Assert.AreEqual(AuctionType.WTB, result.Items[0].AuctionType);
            Assert.AreEqual("Withered Leather Leggings", result.Items[0].Name);
            Assert.AreEqual(2000, result.Items[0].Price);
            Assert.AreEqual(1, result.Items.Count);
        }

        [TestMethod]
        public void Parse8()
        {
            var result = discordAuctionParse.Parse("Naskuu auctions, 'WTS lots of stacks of Bone Chips 8pp/stack'");
            Assert.AreEqual("Naskuu", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("lots of stacks of Bone Chips", result.Items[0].Name);
            Assert.AreEqual(8, result.Items[0].Price);
            Assert.AreEqual(1, result.Items.Count);
        }

        [TestMethod]
        public void Parse9()
        {
            var result = discordAuctionParse.Parse("Waner auctions, 'WTS Silver Chitin Wristband 4k (got 2) Flawless Diamond 2k (got a few)'");
            Assert.AreEqual("Waner", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Silver Chitin Wristband", result.Items[0].Name);
            Assert.AreEqual(4000, result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Flawless Diamond", result.Items[1].Name);
            Assert.AreEqual(2000, result.Items[1].Price);
        }

        [TestMethod]
        public void Parse10()
        {
            var result = discordAuctionParse.Parse("Sorco auctions, 'WTS Black Sapphire Velium Necklace 1.1k : Black Sapphire Electrum Earring 675p : Velium Fire Wedding Ring 425 '");
            Assert.AreEqual("Sorco", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Black Sapphire Velium Necklace", result.Items[0].Name);
            Assert.AreEqual(1100, result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Black Sapphire Electrum Earring", result.Items[1].Name);
            Assert.AreEqual(675, result.Items[1].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[2].AuctionType);
            Assert.AreEqual("Velium Fire Wedding Ring", result.Items[2].Name);
            Assert.AreEqual(425, result.Items[2].Price);
        }
    }
}
