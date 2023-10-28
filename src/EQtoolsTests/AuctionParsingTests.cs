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
            Assert.IsNull(result);
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

        [TestMethod]
        public void Parse11()
        {
            var result = discordAuctionParse.Parse("Legdays auctions, 'WTS Velium Crystal Staff 16k Flawless Diamond 2.5k/ea Coldain Skin Boots 1.8k Earring of Essence 1.5k Tolapumj's Robe 1.5k Black Ice Boots 400p Iksar Hide Boots 400p'");
            Assert.AreEqual("Legdays", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Velium Crystal Staff", result.Items[0].Name);
            Assert.AreEqual(16000, result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Flawless Diamond", result.Items[1].Name);
            Assert.AreEqual(2500, result.Items[1].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[2].AuctionType);
            Assert.AreEqual("Coldain Skin Boots", result.Items[2].Name);
            Assert.AreEqual(1800, result.Items[2].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[3].AuctionType);
            Assert.AreEqual("Earring of Essence", result.Items[3].Name);
            Assert.AreEqual(1500, result.Items[3].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[4].AuctionType);
            Assert.AreEqual("Tolapumj's Robe", result.Items[4].Name);
            Assert.AreEqual(1500, result.Items[4].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[5].AuctionType);
            Assert.AreEqual("Black Ice Boots", result.Items[5].Name);
            Assert.AreEqual(400, result.Items[5].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[6].AuctionType);
            Assert.AreEqual("Iksar Hide Boots", result.Items[6].Name);
            Assert.AreEqual(400, result.Items[6].Price);
        }

        [TestMethod]
        public void Parse12()
        {
            var result = discordAuctionParse.Parse("Archives auctions, 'WTS Giant Warrior Helmet x 4 - 100p ea'");
            Assert.AreEqual("Archives", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Giant Warrior Helmet", result.Items[0].Name);
            Assert.IsNull(result.Items[0].Price);
            Assert.AreEqual(1, result.Items.Count);
        }

        [TestMethod]
        public void Parse13()
        {
            var result = discordAuctionParse.Parse("Rickjaames auctions, 'WTB shield of the stalwart seas 1500 '");
            Assert.AreEqual("Rickjaames", result.Player);
            Assert.AreEqual(AuctionType.WTB, result.Items[0].AuctionType);
            Assert.AreEqual("shield of the stalwart seas", result.Items[0].Name);
            Assert.AreEqual(1500, result.Items[0].Price);
            Assert.AreEqual(1, result.Items.Count);
        }

        [TestMethod]
        public void Parse14()
        {
            var result = discordAuctionParse.Parse("Jalc auctions, 'WTS Platinum Fire Wedding Ring, Loam Encrusted Lined Shoes, Spell: Eye of Tallon, Shark Skin, Rune of Velious, Nilitim's Grimoire Pg. 115, Nilitim's Grimoire Pg. 400 Nilitim's Grimoire Pg. 378, Words of Acquisition (Beza), Rune of Ap`Sagor, Opal, Spell: Color Skew, Spell: Pillage Enchantment, Spell: Recant Magic, Spell: Boon of the Clear Mind, Breath of Ro, Essence of Rathe, Rune of the Cyclone, Rune of Rathe, Words of Haunting, Spell: Recant Magic, Words of Haunting, Tears of Prexus by T1'");
            Assert.AreEqual("Jalc", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Platinum Fire Wedding Ring", result.Items[0].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Loam Encrusted Lined Shoes", result.Items[1].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[2].AuctionType);
            Assert.AreEqual("Spell: Eye of Tallon", result.Items[2].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[3].AuctionType);
            Assert.AreEqual("Shark Skin", result.Items[3].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[4].AuctionType);
            Assert.AreEqual("Rune of Velious", result.Items[4].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[5].AuctionType);
            Assert.AreEqual("Nilitim's Grimoire Pg. 115", result.Items[5].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[6].AuctionType);
            Assert.AreEqual("Rune of Ap`Sagor", result.Items[6].Name);
            Assert.IsNull(result.Items[0].Price);
        }

        [TestMethod]
        public void Parse15()
        {
            var result = discordAuctionParse.Parse("Zteck auctions, 'WTS 3x Words of Haunting | 2x Salil's Writ Pg. 90 | 1x Words of Absorption | 1x Words of Dark Paths | 1x Words of Refuge | 1x Rune of Arrest - PST'");
            Assert.AreEqual("Zteck", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Words of Haunting", result.Items[0].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Salil's Writ Pg. 90", result.Items[1].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[2].AuctionType);
            Assert.AreEqual("Words of Absorption", result.Items[2].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[3].AuctionType);
            Assert.AreEqual("Words of Dark Paths", result.Items[3].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[4].AuctionType);
            Assert.AreEqual("Words of Refuge", result.Items[4].Name);
            Assert.IsNull(result.Items[0].Price);
        }

        [TestMethod]
        public void Parse16()
        {
            var result = discordAuctionParse.Parse("Donkeee auctions, 'WTS Idol of Woven Grass 20p Iron Bound Tome 50p'");
            Assert.AreEqual("Donkeee", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Idol of Woven Grass", result.Items[0].Name);
            Assert.AreEqual(20, result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Iron Bound Tome", result.Items[1].Name);
            Assert.AreEqual(50, result.Items[1].Price);
        }

        [TestMethod]
        public void Parse17()
        {
            var result = discordAuctionParse.Parse("Fuxi auctions, 'WTS Words of Burnishing / Nilitim's Grimoire Pg. 300 x3 / Nilitim's Grimoire Pg. 116 / Nilitim's Grimoire Pg. 115 / Nilitim's Grimoire Pg. 35 / Salil's Writ Pg. 174 L 5pp ea last call pst'");
            Assert.AreEqual("Fuxi", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Words of Burnishing", result.Items[0].Name);
            Assert.IsNull(result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Nilitim's Grimoire Pg. 300", result.Items[1].Name);
            Assert.IsNull(result.Items[1].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[2].AuctionType);
            Assert.AreEqual("Nilitim's Grimoire Pg. 116", result.Items[2].Name);
            Assert.IsNull(result.Items[2].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[3].AuctionType);
            Assert.AreEqual("Nilitim's Grimoire Pg. 115", result.Items[3].Name);
            Assert.IsNull(result.Items[3].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[4].AuctionType);
            Assert.AreEqual("Nilitim's Grimoire Pg. 35", result.Items[4].Name);
            Assert.IsNull(result.Items[4].Price);
        }

        [TestMethod]
        public void Parse18()
        {
            var result = discordAuctionParse.Parse("Cynnen auctions, 'WTS Spell: Pillar of Lightning 50p l Sarnak-Hide Mask 50p l Arctic Wyvern Hide 300p/stack l Cobalt Drake Hide 100p'");
            Assert.AreEqual("Cynnen", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Spell: Pillar of Lightning", result.Items[0].Name);
            Assert.AreEqual(50, result.Items[0].Price);
        }

        [TestMethod]
        public void Parse19()
        {
            var result = discordAuctionParse.Parse("Dulcea auctions, 'WTS Leather Padding stacksx 2-175p each'");
            Assert.IsNull(result);
        }
    }
}
