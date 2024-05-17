using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQToolTests
{
    [TestClass]
    public class ChChainTests
    {
        private readonly IContainer container;
        public ChChainTests()
        {
            container = DI.Init();
        }

        [TestMethod]
        public void Parse1()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Curaja shouts, 'GG 014 CH -- Wreckognize'");
            Assert.AreEqual(d.Recipient, "Wreckognize");
            Assert.AreEqual(d.Caster, "Curaja");
            Assert.AreEqual(d.Position, "014");
            Assert.AreEqual(d.RecipientGuild, "GG");
        }

        [TestMethod]
        public void Parse2()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox shouts, 'GG 001 CH -- Beefwich'");
            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "001");
            Assert.AreEqual(d.RecipientGuild, "GG");
        }

        [TestMethod]
        public void Parse4()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox shouts, 'GG 001 CH --Beefwich'");
            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "001");
            Assert.AreEqual(d.RecipientGuild, "GG");
        }

        [TestMethod]
        public void Parse41()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox shouts, 'CH - Beefwich - 001'");
            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "001");
        }

        [TestMethod]
        public void ParseRamp1()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox shouts, 'CA RAMP1 CH --Beefwich'");
            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "RAMP1");
            Assert.AreEqual(d.RecipientGuild, "CA");
        }

        [TestMethod]
        public void ParseRamp2()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox shouts, 'CA RAMP2 CH --Beefwich'");
            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "RAMP2");
            Assert.AreEqual(d.RecipientGuild, "CA");
        }

        [TestMethod]
        public void ParseRamp3()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox shouts, 'RAMP2 CH --Beefwich'");
            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "RAMP2");
            Assert.IsTrue(string.IsNullOrWhiteSpace(d.RecipientGuild));
        }

        [TestMethod]
        public void Parse40()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox shouts, 'GG 001 CH --Beefwich' 001");
            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "001");
            Assert.AreEqual(d.RecipientGuild, "GG");
        }

        [TestMethod]
        public void Parse42()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox shouts, 'CH - name - 001'");
            Assert.AreEqual(d.Recipient, "name");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "001");
            Assert.AreEqual(d.RecipientGuild, string.Empty);
        }

        [TestMethod]
        public void Parse3()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Vaeric tells the guild, 'Currently signed up as 001 in CH chain'");
            Assert.IsNotNull(d);
        }

        [TestMethod]
        public void Parse31()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Vaeric tells the guild, 'Currently signed up as in CH chain'");
            Assert.IsNull(d);
        }

        [TestMethod]
        public void Parse5()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Wartburg says out of character, 'CA 004 CH -- Sam'");
            Assert.AreEqual(d.Recipient, "Sam");
            Assert.AreEqual(d.Caster, "Wartburg");
            Assert.AreEqual(d.Position, "004");
            Assert.AreEqual(d.RecipientGuild, "CA");
        }

        [TestMethod]
        public void Parse51()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Wartburg says out of character, '004 CH - Sam'");
            Assert.AreEqual(d.Recipient, "Sam");
            Assert.AreEqual(d.Caster, "Wartburg");
            Assert.AreEqual(d.Position, "004");
        }

        [TestMethod]
        public void Parse6()
        {
            var service = container.Resolve<ChParser>();
            var d = service.ChCheck("Hanbox tells the guild, 'GG 001 CH --Beefwich'");
            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "001");
            Assert.AreEqual(d.RecipientGuild, "GG");
        }

        [TestMethod]
        public void Parse7()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                ChChainTagOverlay = "GG"
            };
            var d = service.ChCheck("Hanbox tells the guild, 'GG 001 CH --Beefwich'");

            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "001");
            Assert.AreEqual(d.RecipientGuild, "GG");
        }

        [TestMethod]
        public void Parse8()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                ChChainTagOverlay = "GG"
            };
            var d = service.ChCheck("Hanbox tells the guild, 'CA 001 CH --Beefwich'");
            Assert.IsNull(d);
        }

        [TestMethod]
        public void Parse9()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                ChChainTagOverlay = "GGG"
            };
            var d = service.ChCheck("Hanbox tells the raid, 'GGG 001 CH --Beefwich'");

            Assert.AreEqual(d.Recipient, "Beefwich");
            Assert.AreEqual(d.Caster, "Hanbox");
            Assert.AreEqual(d.Position, "001");
            Assert.AreEqual(d.RecipientGuild, "GGG");
        }

        [TestMethod]
        public void Parse10()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                ChChainTagOverlay = "GGG"
            };
            var d = service.ChCheck("Amberel tells the raid,  'GGG CH - Asirk - 10 s'");
            Assert.IsNull(d);
        }

        [TestMethod]
        public void Parse11()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                ChChainTagOverlay = "GGG"
            };
            var d = service.ChCheck("Windarie tells the group, 'Bufzyn 111 --- CH on << Tinialita  >> --- 111'");
            Assert.IsNull(d);
        }

        [TestMethod]
        public void Parse12()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric,
                ChChainTagOverlay = "CA"
            };
            var d = service.ChCheck("You say out of character, 'CA 002 CH -- Aaryk'");
            Assert.AreEqual(d.Recipient, "Aaryk");
            Assert.AreEqual(d.Caster, "You");
            Assert.AreEqual(d.Position, "002");
            Assert.AreEqual(d.RecipientGuild, "CA");
        }

        [TestMethod]
        public void Parse13()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("Kaboomslang -> Distributin: ch plz");
            Assert.IsNull(d);
        }

        [TestMethod]
        public void Parse14()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("You told someone, 'when CH chains are e a 1-2 full rounds of max dmg hits though if u can'");
            Assert.IsNull(d);
        }

        [TestMethod]
        public void Parse15()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("somecleric tells the guild, '003 - CH 5T'");
            Assert.IsNull(d);
        }

        [TestMethod]
        public void Parse16()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("Windarie auctions, '111 --- CH << Mandair  >> --- 111'");
            Assert.AreEqual(d.Recipient, "Mandair");
            Assert.AreEqual(d.Caster, "Windarie");
            Assert.AreEqual(d.Position, "111");
        }

        [TestMethod]
        public void Parse17()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("Mutao auctions, '777 CH <>> Mandair <<> 777'");
            Assert.AreEqual(d.Recipient, "Mandair");
            Assert.AreEqual(d.Caster, "Mutao");
            Assert.AreEqual(d.Position, "777");
        }

        [TestMethod]
        public void Parse18()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("Mutao auctions, 'AAA CH <>> Mandair <<> AAA'");
            Assert.AreEqual(d.Recipient, "Mandair");
            Assert.AreEqual(d.Caster, "Mutao");
            Assert.AreEqual(d.Position, "AAA");
        }

        [TestMethod]
        public void Parse19()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("Mutao auctions, 'GGG AAA CH <>> Mandair <<> AAA'");
            Assert.AreEqual(d.Recipient, "Mandair");
            Assert.AreEqual(d.Caster, "Mutao");
            Assert.AreEqual(d.Position, "AAA");
            Assert.AreEqual(d.RecipientGuild, "GGG");
        }

        [TestMethod]
        public void Parse20()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("Mutao auctions, 'AAA CH <>> Mandair <<> AAA'");
            Assert.AreEqual(d.Recipient, "Mandair");
            Assert.AreEqual(d.Caster, "Mutao");
            Assert.AreEqual(d.Position, "AAA");
            Assert.AreEqual(d.RecipientGuild, string.Empty);
        }

        [TestMethod]
        public void Parse21()
        {
            var service = container.Resolve<ChParser>();
            var player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 54,
                PlayerClass = PlayerClasses.Cleric
            };
            var d = service.ChCheck("Mutao auctions, 'AAA CH <>> Mandair <<>'");
            Assert.AreEqual(d.Recipient, "Mandair");
            Assert.AreEqual(d.Caster, "Mutao");
            Assert.AreEqual(d.Position, "AAA");
            Assert.AreEqual(d.RecipientGuild, string.Empty);
        }

        [TestMethod]
        public void Parse22()
        {
            var chaindata = new ChainData();
            var chain = new ChParser.ChParseData();
            chaindata.YourChainOrder = "bbb";
            chaindata.HighestOrder = "zzz";
            chain.Position = "aaa";
            Assert.IsTrue(CHService.ShouldWarnOfChain(chaindata, chain));
            chaindata.HighestOrder = "bbb";
            chain.Position = "aaa";
            chaindata.YourChainOrder = "bbb";
            Assert.IsTrue(CHService.ShouldWarnOfChain(chaindata, chain));
            chaindata.HighestOrder = "ccc";
            chain.Position = "aaa";
            chaindata.YourChainOrder = "bbb";
            Assert.IsTrue(CHService.ShouldWarnOfChain(chaindata, chain));
            chaindata.HighestOrder = "ccc";
            chain.Position = "ccc";
            chaindata.YourChainOrder = "bbb";
            Assert.IsFalse(CHService.ShouldWarnOfChain(chaindata, chain));
            chaindata.HighestOrder = "aaa";
            chain.Position = "zzz";
            chaindata.YourChainOrder = "bbb";
            Assert.IsFalse(CHService.ShouldWarnOfChain(chaindata, chain));
            chaindata.HighestOrder = "bbb";
            chain.Position = "bbb";
            chaindata.YourChainOrder = "aaa";
            Assert.IsFalse(CHService.ShouldWarnOfChain(chaindata, chain));

            chaindata.HighestOrder = "004";
            chain.Position = "003";
            chaindata.YourChainOrder = "004";
            Assert.IsTrue(CHService.ShouldWarnOfChain(chaindata, chain));

            chaindata.HighestOrder = "004";
            chain.Position = "004";
            chaindata.YourChainOrder = "002";
            Assert.IsFalse(CHService.ShouldWarnOfChain(chaindata, chain));

            chaindata.HighestOrder = "004";
            chain.Position = "004";
            chaindata.YourChainOrder = "002";
            Assert.IsFalse(CHService.ShouldWarnOfChain(chaindata, chain));

            chaindata.HighestOrder = "004";
            chain.Position = "001";
            chaindata.YourChainOrder = "002";
            Assert.IsTrue(CHService.ShouldWarnOfChain(chaindata, chain));

            chaindata.HighestOrder = "004";
            chain.Position = "001";
            chaindata.YourChainOrder = "003";
            Assert.IsFalse(CHService.ShouldWarnOfChain(chaindata, chain));

            chaindata.HighestOrder = "004";
            chain.Position = "001";
            chaindata.YourChainOrder = "001";
            Assert.IsFalse(CHService.ShouldWarnOfChain(chaindata, chain));

            chaindata.HighestOrder = "004";
            chain.Position = "004";
            chaindata.YourChainOrder = "001";
            Assert.IsTrue(CHService.ShouldWarnOfChain(chaindata, chain));

        }
    }
}
