using Autofac;
using EQToolShared.Discord;
using EQToolShared.Enums;
using EQToolTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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

        //[TestMethod]
        //public void ParseDiscordMessage()
        //{
        //    var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DiscordResponse.json");
        //    var resultstring = File.ReadAllText(filepath);
        //    var outs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Message>>(resultstring);
        //    var Emissaryofthule = outs.Where(a => a.embeds.Any(b => b.fields.Any(c => c.Price.HasValue))).Take(4).ToList();
        //    Assert.IsNotNull(Emissaryofthule);
        //}

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
            var item = result.Items.FirstOrDefault(a => a.Name == "Nathsar Bracer");
            Assert.AreEqual(AuctionType.WTB, item.AuctionType);
            Assert.AreEqual(200, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Nathsar Helm");
            Assert.AreEqual(AuctionType.WTB, item.AuctionType);
            Assert.AreEqual(400, item.Price);
        }

        [TestMethod]
        public void Parse4()
        {
            var result = discordAuctionParse.Parse("Darkinvader auctions, 'WTS Skyfury Scimitar / Othmir Fur x15 / Black Ice Leggings / Spell: Focus of Spirit / Spell: Death Pact / Crustacean Shell Gauntlets / Crustacean Shell Shield'");
            Assert.AreEqual("Darkinvader", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Skyfury Scimitar");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            result.Items.FirstOrDefault(a => a.Name == "Othmir Fur");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            result.Items.FirstOrDefault(a => a.Name == "Black Ice Leggings");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            result.Items.FirstOrDefault(a => a.Name == "Spell: Focus of Spirit");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            result.Items.FirstOrDefault(a => a.Name == "Crustacean Shell Shield");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);
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
            var item = result.Items.FirstOrDefault(a => a.Name == "Bone Chips");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(8, item.Price);
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

            var item = result.Items.FirstOrDefault(a => a.Name == "Black Sapphire Velium Necklace");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(1100, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Black Sapphire Electrum Earring");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(675, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Velium Fire Wedding Ring");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(425, item.Price);
        }

        [TestMethod]
        public void Parse11()
        {
            var result = discordAuctionParse.Parse("Legdays auctions, 'WTS Velium Crystal Staff 16k Flawless Diamond 2.5k/ea Coldain Skin Boots 1.8k Earring of Essence 1.5k Tolapumj's Robe 1.5k Black Ice Boots 400p Iksar Hide Boots 400p'");
            Assert.AreEqual("Legdays", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Velium Crystal Staff");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(16000, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Flawless Diamond");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(2500, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Coldain Skin Boots");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(1800, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Earring of Essence");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(1500, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Tolapumj's Robe");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(1500, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Earring of Essence");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(1500, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Black Ice Boots");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(400, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Iksar Hide Boots");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(400, item.Price);
        }

        [TestMethod]
        public void Parse12()
        {
            var result = discordAuctionParse.Parse("Archives auctions, 'WTS Giant Warrior Helmet x 4 - 100p ea // Words of Collection (Beza) 56pp'");
            Assert.AreEqual("Archives", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Giant Warrior Helmet");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(100, item.Price);
        }

        [TestMethod]
        public void Parse13()
        {
            var result = discordAuctionParse.Parse("Rickjaames auctions, 'WTB shield of the stalwart seas 1500 '");
            Assert.AreEqual("Rickjaames", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Shield of the Stalwart Seas");
            Assert.AreEqual(AuctionType.WTB, item.AuctionType);
            Assert.AreEqual(1500, item.Price);
        }

        [TestMethod]
        public void Parse14()
        {
            var result = discordAuctionParse.Parse("Jalc auctions, 'WTS Platinum Fire Wedding Ring, Loam Encrusted Lined Shoes, Spell: Eye of Tallon, Shark Skin, Rune of Velious, Nilitim's Grimoire Pg. 115, Nilitim's Grimoire Pg. 400 Nilitim's Grimoire Pg. 378, Words of Acquisition (Beza), Rune of Ap`Sagor, Opal, Spell: Color Skew, Spell: Pillage Enchantment, Spell: Recant Magic, Spell: Boon of the Clear Mind, Breath of Ro, Essence of Rathe, Rune of the Cyclone, Rune of Rathe, Words of Haunting, Spell: Recant Magic, Words of Haunting, Tears of Prexus by T1'");
            Assert.AreEqual("Jalc", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Platinum Fire Wedding Ring");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Loam Encrusted Lined Shoes");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Spell: Eye of Tallon");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);


        }

        [TestMethod]
        public void Parse15()
        {
            var result = discordAuctionParse.Parse("Zteck auctions, 'WTS 3x Words of Haunting | 2x Salil's Writ Pg. 90 | 1x Words of Absorption | 1x Words of Dark Paths | 1x Words of Refuge | 1x Rune of Arrest - PST'");
            Assert.AreEqual("Zteck", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Words of Haunting");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Salil's Writ Pg. 90");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Words of Absorption");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Words of Dark Paths");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Words of Refuge");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);
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

            var item = result.Items.FirstOrDefault(a => a.Name == "Words of Burnishing");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Nilitim's Grimoire Pg. 300");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Nilitim's Grimoire Pg. 116");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Nilitim's Grimoire Pg. 115");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Nilitim's Grimoire Pg. 116");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Nilitim's Grimoire Pg. 116");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Nilitim's Grimoire Pg. 35");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Salil's Writ Pg. 174");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);
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
            Assert.AreEqual("Dulcea", result.Player);
            var item = result.Items.FirstOrDefault(a => a.Name == "Leather Padding");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);
        }


        [TestMethod]
        public void Parse20()
        {
            var result = discordAuctionParse.Parse("Stockroom auctions, 'WTS Spell: Minor Conjuration: Air 100 | Spell: Reckoning 50.'");
            Assert.AreEqual("Stockroom", result.Player);
            Assert.AreEqual(AuctionType.WTS, result.Items[0].AuctionType);
            Assert.AreEqual("Spell: Minor Conjuration: Air", result.Items[0].Name);
            Assert.AreEqual(100, result.Items[0].Price);

            Assert.AreEqual(AuctionType.WTS, result.Items[1].AuctionType);
            Assert.AreEqual("Spell: Reckoning", result.Items[1].Name);
            Assert.AreEqual(50, result.Items[1].Price);
        }

        [TestMethod]
        public void Parse21()
        {
            var result = discordAuctionParse.Parse("Vermeil auctions, 'WTS Mrylokar's Vambraces 2k / Crystalline Silk 200 a stack / Blue Diamond 350 / Spell: Evacuate 100 / Othmir Fur Cloak treefiddy.'");
            Assert.AreEqual("Vermeil", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Mrylokar's Vambraces");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(2000, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Crystalline Silk");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(200, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Blue Diamond");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(350, item.Price);

        }

        [TestMethod]
        public void Parse22()
        {
            var result = discordAuctionParse.Parse("Vermeil auctions, 'WTS Mrylokar's Vambraces 2k / Giant Warrior Helmet 100 (2 left) / Crystalline Silk 200 a stack (3 stacks)'");
            Assert.AreEqual("Vermeil", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Mrylokar's Vambraces");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(2000, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Crystalline Silk");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(200, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Giant Warrior Helmet");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(100, item.Price);
        }

        [TestMethod]
        public void Parse23()
        {
            var result = discordAuctionParse.Parse("Grecc auctions, 'WTS Orc Scalpx50 4p/ea - 200p for all Shiny Brass Idol 400p'");
            Assert.AreEqual("Grecc", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Orc Scalp");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(4, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Shiny Brass Idol");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(400, item.Price);
        }

        [TestMethod]
        public void Parse24()
        {
            var result = discordAuctionParse.Parse("Myule auctions, 'WTS Jaundice Gem x8, Crushed Chrysolite, Flawed Topaz x7, Nephrite, Flawed Sea Sapphire x5, Flawed Emerald x3'");
            Assert.AreEqual("Myule", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Jaundice Gem");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Crushed Chrysolite");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Flawed Topaz");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Nephrite");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Flawed Sea Sapphire");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Flawed Emerald");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);
        }

        [TestMethod]
        public void Parse25()
        {
            var result = discordAuctionParse.Parse("Dialeah auctions, 'WTS Djarns Amethyst Ring 5k /// Mithril Vambraces 500p // Velium Blue Diamond Bracelet 1k // '");
            Assert.AreEqual("Dialeah", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Djarns Amethyst Ring");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(5000, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Mithril Vambraces");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(500, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Velium Blue Diamond Bracelet");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(1000, item.Price);
        }

        [TestMethod]
        public void Parse26()
        {
            var result = discordAuctionParse.Parse("Liquide auctions, 'WTS  Worker Sledgemallet / OT Hammer / Puppet Strings clicks 3.5k (lvl 51+) OR 4k for my Hammer Guarantee™, Lodi Map LR (future)'");
            Assert.AreEqual("Liquide", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Worker Sledgemallet");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Puppet Strings");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Rings");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            Assert.AreEqual(3, result.Items.Count);
        }


        [TestMethod]
        public void Parse27()
        {
            var result = discordAuctionParse.Parse("Rahtin auctions, 'WTS 10 Dose Potion of Stinging Wort'");
            Assert.AreEqual("Rahtin", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "10 Dose Potion of Stinging Wort");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);
        }

        [TestMethod]
        public void Parse28()
        {
            var result = discordAuctionParse.Parse("Transference auctions, 'WTS Spell: Wave of Healing ~ Spell: Aegolism ~ Spell: Heroic Bond ~ Spell: Blizzard ~ Spell: Call of the Predator ~ Spell: Enticement of Flame'");
            Assert.AreEqual("Transference", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Spell: Wave of Healing");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Spell: Aegolism");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Spell: Heroic Bond");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Spell: Blizzard");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Spell: Enticement of Flame");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);
        }

        [TestMethod]
        public void Parse29()
        {
            var result = discordAuctionParse.Parse("Rightnow auctions, 'WTB Mage Spell  Burnout lV & Eye of  Tallon PST'");
            Assert.AreEqual("Rightnow", result.Player);
            Assert.AreEqual(1, result.Items.Count);
        }


        [TestMethod]
        public void Parse30()
        {
            var result = discordAuctionParse.Parse("Telson auctions, 'WTS: Bone Chips 9pp/stack | Breath of Solusek 15p | Glove of Rallos Zek 15p(3)'");
            Assert.AreEqual("Telson", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Bone Chips");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(9, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Breath of Solusek");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(15, item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "Glove of Rallos Zek");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.AreEqual(15, item.Price);
        }

        [TestMethod]
        public void Parse31()
        {
            var result = discordAuctionParse.Parse("Happendy auctions, 'Wts Sea Dragon Meat | Fishbone Earring | 10 Dose Potion of Antiweight | 10 Dose Blood of the Wolf | 10 Dose Potion of Stinging Wort | 00387800000000000000000000000000000007A3FFC64Potion of the Swamp '");
            Assert.AreEqual("Telson", result.Player);

            var item = result.Items.FirstOrDefault(a => a.Name == "Sea Dragon Meat");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);

            item = result.Items.FirstOrDefault(a => a.Name == "10 Dose Potion of Stinging Wort");
            Assert.AreEqual(AuctionType.WTS, item.AuctionType);
            Assert.IsNull(item.Price);
        }

    }
}
