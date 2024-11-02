using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Handlers;
using EQTool.ViewModels;
using EQToolShared.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace EQtoolsTests
{
    [TestClass]
    public class SlainHandlerTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private int CalledCounter = 0;

        public SlainHandlerTests()
        {
            logParser = container.Resolve<LogParser>();
            logEvents = container.Resolve<LogEvents>();
            activePlayer = container.Resolve<ActivePlayer>();
            _ = container.Resolve<IEnumerable<BaseHandler>>();
            _ = container.Resolve<IEnumerable<IEqLogParseHandler>>();
            activePlayer.Player.Level = 54;
            activePlayer.Player.PlayerClass = PlayerClasses.Cleric;
        }

        [TestMethod]
        public void HappyPathAllThreeMessages()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                Assert.AreEqual(e.Victim, "a frost giant scout");
                Assert.AreEqual(e.Killer, "You");
                CalledCounter++;
            };
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            Assert.AreEqual(CalledCounter, 1);
        }

        [TestMethod]
        public void SlainInMiddle()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                Assert.AreEqual(e.Victim, "a frost giant scout");
                Assert.AreEqual(e.Killer, "You");
                CalledCounter++;
            };

            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            Assert.AreEqual(CalledCounter, 1);
        }

        [TestMethod]
        public void SlainAtEnd()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                Assert.AreEqual(e.Victim, "a frost giant scout");
                Assert.AreEqual(e.Killer, "You");
                CalledCounter++;
            };

            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            Assert.AreEqual(CalledCounter, 1);
        }

        [TestMethod]
        public void Slain0()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                CalledCounter++;
            };

            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Lilrez begins to cast a spell.", DateTime.Now);
            Assert.AreEqual(CalledCounter, 1);
        }

        [TestMethod]
        public void Slain1()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                Assert.AreEqual(e.Victim, "a skeleton");
                Assert.AreEqual(e.Killer, "You");
                CalledCounter++;
            };

            logParser.Push("You crush a skeleton for 46 points of damage.", DateTime.Now);
            logParser.Push("You have slain a skeleton!", DateTime.Now);
            logParser.Push("Your Location is -0.26, 1844.07, -14.98", DateTime.Now);
            Assert.AreEqual(CalledCounter, 1);
        }

        [TestMethod]
        public void Slain2()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                Assert.AreEqual(e.Victim, "Robobard");
                Assert.AreEqual(e.Killer, "Sontalak");
                CalledCounter++;
            };

            logParser.Push("Sontalak claws Robobard for 425 points of damage.", DateTime.Now);
            logParser.Push("Robobard has been slain by Sontalak!", DateTime.Now);
            logParser.Push("Sontalak says 'Ack!  I must be careful not to step on that body, it tastes much better when it is still crunchy, not pulped!'", DateTime.Now);
            Assert.AreEqual(CalledCounter, 1);
        }

        [TestMethod]
        public void Slain3()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                Assert.AreEqual(e.Victim, "Marantula");
                Assert.AreEqual(e.Killer, "an ancient wyvern");
                CalledCounter++;
            };

            logParser.Push("An ancient wyvern hits Marantula for 196 points of damage.", DateTime.Now);
            logParser.Push("Marantula has been slain by an ancient wyvern!", DateTime.Now);
            logParser.Push("Your Location is -388.49, -751.23, 43.72", DateTime.Now);
            Assert.AreEqual(CalledCounter, 1);
        }

        [TestMethod]
        public void Slain5()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                if (e.Victim == "Marantula")
                {
                    Assert.AreEqual(e.Victim, "Marantula");
                    Assert.AreEqual(e.Killer, "an ancient wyvern");
                }
                else
                {
                    Assert.AreEqual(e.Victim, "Gluwen");
                    Assert.AreEqual(e.Killer, "an ancient wyvern");
                }
                CalledCounter++;
            };

            logParser.Push("An ancient wyvern hits Marantula for 196 points of damage.", DateTime.Now);
            logParser.Push("Marantula has been slain by an ancient wyvern!", DateTime.Now);
            logParser.Push("Gluwen has been slain by an ancient wyvern!", DateTime.Now);
            Assert.AreEqual(CalledCounter, 2);
        }

        [TestMethod]
        public void Slain6()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                Assert.AreEqual(e.Victim, "Faction Slain Guess");
                Assert.AreEqual(e.Killer, "You");
                CalledCounter++;
            };

            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("Lilrez begins to cast a spell.", DateTime.Now);
            Assert.AreEqual(CalledCounter, 1);
        }

        [TestMethod]
        public void Slain7()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                if (e.Victim == "Marantula")
                {
                    Assert.AreEqual(e.Victim, "Marantula");
                    Assert.AreEqual(e.Killer, "an ancient wyvern");
                }
                else
                {
                    Assert.AreEqual(e.Victim, "Faction Slain Guess");
                    Assert.AreEqual(e.Killer, "You");
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
            Assert.AreEqual(CalledCounter, 2);
        }

        [TestMethod]
        public void MultipleFactionMessages()
        {
            logEvents.NewSlainEvent += (a, e) =>
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
            Assert.AreEqual(CalledCounter, 3);
        }

        [TestMethod]
        public void Slain9()
        {
            logEvents.NewSlainEvent += (a, e) =>
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
            Assert.AreEqual(CalledCounter, 5);
        }

        [TestMethod]
        public void MultipleExpMessages()
        {
            var expkillcount = 0;
            logEvents.NewSlainEvent += (a, e) =>
            {
                if (e.Victim == "a frost giant scout")
                {
                    CalledCounter++;
                }
                else
                {
                    Assert.AreEqual(e.Victim, "Experience Slain Guess");
                    Assert.AreEqual(e.Killer, "You");
                    expkillcount++;
                }
            };
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);

            Assert.AreEqual(expkillcount, 2);
            Assert.AreEqual(CalledCounter, 1);
        }


        [TestMethod]
        public void MultipleSlainMessages()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                CalledCounter++;
            };

            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);

            Assert.AreEqual(CalledCounter, 3);
        }


        [TestMethod]
        public void MultipleFactionMessagesDifferent ()
        {
            logEvents.NewSlainEvent += (a, e) =>
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
            Assert.AreEqual(CalledCounter, 3);
        }


        [TestMethod]
        public void IgnoreQuestTurnins()
        {
            logEvents.NewSlainEvent += (a, e) =>
            {
                CalledCounter++;
            };
            logParser.Push("Jaerlin pierces a Drakkel Dire Wolf for 42 points of damage.", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            logParser.Push("Captain Ashlan says, 'Great work! Maybe you can help us out again sometime?'", DateTime.Now);
            Assert.AreEqual(CalledCounter, 0);
        }
    }
}
