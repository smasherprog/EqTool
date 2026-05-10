using Autofac;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class SlainHandlerTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly SpellWindowViewModel spellWindowViewModel;
        private int CalledCounter = 0;

        public SlainHandlerTests()
        {
            logParser = container.Resolve<LogParser>();
            logEvents = container.Resolve<LogEvents>();
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            player.Player.Level = 54;
            player.Player.PlayerClass = PlayerClasses.Cleric;
        }

        [TestMethod]
        public void HappyPathAllThreeMessages()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                Assert.AreEqual("a frost giant scout", e.Victim);
                Assert.AreEqual("You", e.Killer);
                CalledCounter++;
            };
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }

        [TestMethod]
        public void HappyPathAllThreeMessagesWithNumberIncrement()
        {
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            var spells = container.Resolve<SpellWindowViewModel>();

            Assert.HasCount(3 + Zones.Boats.Count, spells.SpellList);
        }

        [TestMethod]
        public void YouSlainShouldNotShowUp()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                Assert.AreEqual("a frost giant scout", e.Victim);
                Assert.AreEqual("You", e.Killer);
                CalledCounter++;
            };
            logParser.Push("You have been slain by Ajorek the Crimson Fang!", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            Assert.AreEqual(0, CalledCounter);
        }

        [TestMethod]
        public void SlainInMiddle()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                Assert.AreEqual("a frost giant scout", e.Victim);
                Assert.AreEqual("You", e.Killer);
                CalledCounter++;
            };

            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }


        [TestMethod]
        public void SlainAtEnd()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                Assert.AreEqual("a frost giant scout", e.Victim);
                Assert.AreEqual("You", e.Killer);
                CalledCounter++;
            };

            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }

        [TestMethod]
        public void Slain0()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                CalledCounter++;
            };

            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Lilrez begins to cast a spell.", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }

        [TestMethod]
        public void Slain1()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                Assert.AreEqual("a skeleton", e.Victim);
                Assert.AreEqual("You", e.Killer);
                CalledCounter++;
            };

            logParser.Push("You crush a skeleton for 46 points of damage.", DateTime.Now);
            logParser.Push("You have slain a skeleton!", DateTime.Now);
            logParser.Push("Your Location is -0.26, 1844.07, -14.98", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }

        [TestMethod]
        public void Slain2()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                Assert.AreEqual("Robobard", e.Victim);
                Assert.AreEqual("Sontalak", e.Killer);
                CalledCounter++;
            };

            logParser.Push("Sontalak claws Robobard for 425 points of damage.", DateTime.Now);
            logParser.Push("Robobard has been slain by Sontalak!", DateTime.Now);
            logParser.Push("Sontalak says 'Ack!  I must be careful not to step on that body, it tastes much better when it is still crunchy, not pulped!'", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }

        [TestMethod]
        public void Slain3()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                Assert.AreEqual("Marantula", e.Victim);
                Assert.AreEqual("an ancient wyvern", e.Killer);
                CalledCounter++;
            };

            logParser.Push("An ancient wyvern hits Marantula for 196 points of damage.", DateTime.Now);
            logParser.Push("Marantula has been slain by an ancient wyvern!", DateTime.Now);
            logParser.Push("Your Location is -388.49, -751.23, 43.72", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }

        [TestMethod]
        public void Slain5()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                if (e.Victim == "Marantula")
                {
                    Assert.AreEqual("Marantula", e.Victim);
                    Assert.AreEqual("an ancient wyvern", e.Killer);
                }
                else
                {
                    Assert.AreEqual("Gluwen", e.Victim);
                    Assert.AreEqual("an ancient wyvern", e.Killer);
                }
                CalledCounter++;
            };

            logParser.Push("An ancient wyvern hits Marantula for 196 points of damage.", DateTime.Now);
            logParser.Push("Marantula has been slain by an ancient wyvern!", DateTime.Now);
            logParser.Push("Gluwen has been slain by an ancient wyvern!", DateTime.Now);
            Assert.AreEqual(2, CalledCounter);
        }

        [TestMethod]
        public void Slain6()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                Assert.AreEqual("Faction Slain", e.Victim);
                Assert.AreEqual("You", e.Killer);
                CalledCounter++;
            };

            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Lilrez begins to cast a spell.", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }

        [TestMethod]
        public void Slain7()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                if (e.Victim == "Marantula")
                {
                    Assert.AreEqual("Marantula", e.Victim);
                    Assert.AreEqual("an ancient wyvern", e.Killer);
                }
                else
                {
                    Assert.AreEqual("Faction Slain", e.Victim);
                    Assert.AreEqual("You", e.Killer);
                }
                CalledCounter++;
            };
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Lilrez begins to cast a spell.", DateTime.Now);
            logParser.Push("An ancient wyvern hits Marantula for 196 points of damage.", DateTime.Now);
            logParser.Push("Marantula has been slain by an ancient wyvern!", DateTime.Now);
            Assert.AreEqual(2, CalledCounter);
        }

        [TestMethod]
        public void MultipleFactionMessages()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                CalledCounter++;
            };
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);
            Assert.AreEqual(3, CalledCounter);
        }

        [TestMethod]
        public void Slain9()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                CalledCounter++;
            };
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Marantula has been slain by an ancient wyvern!", DateTime.Now);
            logParser.Push("Gluwen has been slain by an ancient wyvern!", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);
            Assert.AreEqual(5, CalledCounter);
        }

        [TestMethod]
        public void MultipleExpMessages()
        {
            var expkillcount = 0;
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                if (e.Victim == "a frost giant scout")
                {
                    CalledCounter++;
                }
                else
                {
                    Assert.AreEqual("Exp Slain", e.Victim);
                    Assert.AreEqual("You", e.Killer);
                    expkillcount++;
                }
            };
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);

            Assert.AreEqual(2, expkillcount);
            Assert.AreEqual(1, CalledCounter);
        }


        [TestMethod]
        public void MultipleSlainMessages()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                CalledCounter++;
            };

            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);

            Assert.AreEqual(3, CalledCounter);
        }


        [TestMethod]
        public void MultipleFactionMessagesDifferent()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                CalledCounter++;
            };
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Knight got better.", DateTime.Now);
            logParser.Push("Your faction standing with Doesntmatter got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);
            Assert.AreEqual(3, CalledCounter);
        }

        [TestMethod]
        public void SlainFactionTest()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                CalledCounter++;
            };
            logParser.Push("You have slain a zol ghoul knight!", DateTime.Now);
            logParser.Push("Your faction standing with FrogloksofGuk got better.", DateTime.Now);
            logParser.Push("Your faction standing with UndeadFrogloksofGuk could not possibly get any worse.", DateTime.Now);
            logParser.Push("Varer judges you amiably -- You would probably win this fight..it's not certain though.", DateTime.Now);

            logParser.Push("Varer slices a wan ghoul knight for 24 points of damage.", DateTime.Now);
            logParser.Push("You have slain a wan ghoul knight!", DateTime.Now);
            logParser.Push("Varer slices a wan ghoul knight for 24 points of damage.", DateTime.Now);
            logParser.Push("You have slain a zol ghoul knight!", DateTime.Now);
            logParser.Push("Your faction standing with FrogloksofGuk got better.", DateTime.Now);
            logParser.Push("Your faction standing with UndeadFrogloksofGuk could not possibly get any worse.", DateTime.Now);
            logParser.Push("Varer judges you amiably -- You would probably win this fight..it's not certain though.", DateTime.Now);
            Assert.AreEqual(3, CalledCounter);
        }

        [TestMethod]
        public void SlainNeriakTest()
        {
            logEvents.ConfirmedDeathEvent += (a, e) =>
            {
                CalledCounter++;
            };
            logParser.Push("You crush Uglan for 19 points of damage.", DateTime.Now);
            logParser.Push("You have slain Uglan!", DateTime.Now);
            logParser.Push("Your faction standing with EldritchCollective got better.", DateTime.Now);
            logParser.Push("Your faction standing with KeepersoftheArt got better.", DateTime.Now);
            logParser.Push("Your faction standing with KingAythoxThex got better.", DateTime.Now);

            logParser.Push("Your faction standing with PrimordialMalice got better.", DateTime.Now);
            logParser.Push("Your faction standing with QueenCristanosThex got worse.", DateTime.Now);
            logParser.Push("Your faction standing with TheDead could not possibly get any worse.", DateTime.Now);
            logParser.Push("You gain party experience!!", DateTime.Now);
            logParser.Push("You fail to locate any food nearby.", DateTime.Now);
            Assert.AreEqual(1, CalledCounter);
        }
    }
}
