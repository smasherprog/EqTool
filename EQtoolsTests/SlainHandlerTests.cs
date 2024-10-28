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
        private readonly bool isCalled = false;

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
            logParser.Push("You have slain a frost giant scout!", DateTime.Now);
            logParser.Push("Your faction standing with ClawsofVeeshan got better.", DateTime.Now);
            logParser.Push("Your faction standing with Coldain got better.", DateTime.Now);
            logParser.Push("Your faction standing with Kromrif got worse.", DateTime.Now);
            logParser.Push("Your faction standing with Kromzek got worse.", DateTime.Now);
            logParser.Push("You gain experience!!", DateTime.Now);
            Assert.IsFalse(isCalled);
        }
    }
}
