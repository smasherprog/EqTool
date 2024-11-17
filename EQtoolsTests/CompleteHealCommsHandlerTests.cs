using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class CompleteHealCommsHandlerTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private bool isCalled = false;

        public CompleteHealCommsHandlerTests()
        {
            logParser = container.Resolve<LogParser>();
            logEvents = container.Resolve<LogEvents>();
            activePlayer = container.Resolve<ActivePlayer>();
            activePlayer.Player.Level = 54;
            activePlayer.Player.PlayerClass = PlayerClasses.Cleric;
        }

        [TestMethod]
        public void Parse0()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                isCalled = true;
            };
            logParser.Push("Curaja shouts, 'ench lfg'", DateTime.Now);
            Assert.IsFalse(isCalled);
        }

        [TestMethod]
        public void Parse1()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Wreckognize");
                Assert.AreEqual(d.Caster, "Curaja");
                Assert.AreEqual(d.Position, "014");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Curaja shouts, 'GG 014 CH -- Wreckognize'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse2()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "001");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'GG 001 CH -- Beefwich'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse4()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "001");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'GG 001 CH --Beefwich'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse41()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "001");
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'CH - Beefwich - 001'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void ParseRamp1()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "RAMP1");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'CA RAMP1 CH --Beefwich'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void ParseRamp2()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "RAMP2");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'CA RAMP2 CH --Beefwich'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void ParseRamp3()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "RAMP2");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'RAMP2 CH --Beefwich'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void ParseRamp4()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "RAMP01");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'RAMP01 CH --Beefwich'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse40()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "001");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'GG 001 CH --Beefwich' 001", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse42()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "name");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "001");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox shouts, 'CH - name - 001'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse3()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.Fail("Should not be called.");
            };
            logParser.Push("Vaeric tells the guild, 'Currently signed up as 001 in CH chain'", DateTime.Now);
        }

        [TestMethod]
        public void Parse31()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.Fail("Should not be called.");
            };
            logParser.Push("Vaeric tells the guild, 'Currently signed up as in CH chain'", DateTime.Now);

        }

        [TestMethod]
        public void Parse5()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Sam");
                Assert.AreEqual(d.Caster, "Wartburg");
                Assert.AreEqual(d.Position, "004");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Wartburg says out of character, 'CA 004 CH -- Sam'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse51()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Sam");
                Assert.AreEqual(d.Caster, "Wartburg");
                Assert.AreEqual(d.Position, "004");
                isCalled = true;
            };
            logParser.Push("Wartburg says out of character, '004 CH - Sam'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse6()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "001");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Hanbox tells the guild, 'GG 001 CH --Beefwich'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse7()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {

                Assert.AreEqual(d.Recipient, "Beefwich");
                Assert.AreEqual(d.Caster, "Hanbox");
                Assert.AreEqual(d.Position, "001");
                Assert.AreEqual(d.Tag, "GG");
                isCalled = true;
            };
            activePlayer.Player.ChChainTagOverlay = "GG";
            logParser.Push("Hanbox tells the guild, 'GG 001 CH --Beefwich'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse8()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.Fail("Should not get here");
            };

            activePlayer.Player.ChChainTagOverlay = "GG";
            logParser.Push("Hanbox tells the guild, 'CA 001 CH --Beefwich'", DateTime.Now);
        }

        [TestMethod]
        public void Parse11()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.Fail("Should not get here");
            };

            activePlayer.Player.ChChainTagOverlay = "GGG";
            logParser.Push("Windarie tells the group, 'Bufzyn 111 --- CH on << Tinialita  >> --- 111'", DateTime.Now);
        }

        [TestMethod]
        public void Parse12()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Aaryk");
                Assert.AreEqual(d.Caster, "You");
                Assert.AreEqual(d.Position, "002");
                Assert.AreEqual(d.Tag, "CA");
                isCalled = true;
            };

            activePlayer.Player.ChChainTagOverlay = "CA";
            logParser.Push("You say out of character, 'CA 002 CH -- Aaryk'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse13()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.Fail("Should not be called");
            };
            logParser.Push("Kaboomslang -> Distributin: ch plz", DateTime.Now);
        }

        [TestMethod]
        public void Parse14()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.Fail("Should not be called");
            };
            logParser.Push("You told someone, 'when CH chains are e a 1-2 full rounds of max dmg hits though if u can'", DateTime.Now);
        }

        [TestMethod]
        public void Parse15()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.Fail("Should not be called");
            };
            logParser.Push("somecleric tells the guild, '003 - CH 5T'", DateTime.Now);
        }

        [TestMethod]
        public void Parse16()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Mandair");
                Assert.AreEqual(d.Caster, "Windarie");
                Assert.AreEqual(d.Position, "111");
                isCalled = true;
            };
            logParser.Push("Windarie auctions, '111 --- CH << Mandair  >> --- 111'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse17()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Mandair");
                Assert.AreEqual(d.Caster, "Mutao");
                Assert.AreEqual(d.Position, "777");
                isCalled = true;
            };
            logParser.Push("Mutao auctions, '777 CH <>> Mandair <<> 777'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse18()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Mandair");
                Assert.AreEqual(d.Caster, "Mutao");
                Assert.AreEqual(d.Position, "AAA");
                isCalled = true;
            };
            logParser.Push("Mutao auctions, 'AAA CH <>> Mandair <<> AAA'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse19()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Mandair");
                Assert.AreEqual(d.Caster, "Mutao");
                Assert.AreEqual(d.Position, "AAA");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Mutao auctions, 'GGG AAA CH <>> Mandair <<> AAA'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse20()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Mandair");
                Assert.AreEqual(d.Caster, "Mutao");
                Assert.AreEqual(d.Position, "AAA");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Mutao auctions, 'AAA CH <>> Mandair <<> AAA'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse21()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "Mandair");
                Assert.AreEqual(d.Caster, "Mutao");
                Assert.AreEqual(d.Position, "AAA");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Mutao auctions, 'AAA CH <>> Mandair <<>'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse22()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "TARGET");
                Assert.AreEqual(d.Caster, "Mutao");
                Assert.AreEqual(d.Position, "AAA");
                Assert.AreEqual(d.Tag, "GG");
                isCalled = true;
            };
            activePlayer.Player.ChChainTagOverlay = "GG";
            logParser.Push("Mutao auctions, 'GG RCH AAA -- TARGET'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse23()
        {
            var chaindata = new ChainData();
            var chain = new CompleteHealEvent();
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

        [TestMethod]
        public void Parse24()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "a shiverback");
                Assert.AreEqual(d.Caster, "Mutao");
                Assert.AreEqual(d.Position, "007");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Mutao auctions, '007 CH --  a shiverback'", DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void Parse26()
        {
            logEvents.CompleteHealEvent += (s, d) =>
            {
                Assert.AreEqual(d.Recipient, "johny");
                Assert.AreEqual(d.Caster, "Mutao");
                Assert.AreEqual(d.Position, "000");
                Assert.AreEqual(d.Tag, string.Empty);
                isCalled = true;
            };
            logParser.Push("Mutao tells the group, 'CH >      johny  '", DateTime.Now);
            Assert.IsTrue(isCalled);
        }
    }
}
