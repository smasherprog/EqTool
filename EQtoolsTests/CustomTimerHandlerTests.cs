using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.Services.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace EQtoolsTests
{
    [TestClass]
    public class CustomTimerHandlerTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly LogEvents logEvents;
        private bool isCalled = false;

        public CustomTimerHandlerTests()
        {
            logParser = container.Resolve<LogParser>();
            logEvents = container.Resolve<LogEvents>();
            _ = container.Resolve<IEnumerable<BaseHandler>>();
            _ = container.Resolve<IEnumerable<IEqLogParser>>();
        }

        [TestMethod]
        public void GetCustomTimerStart()
        {
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual(30 * 60, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("StupidGoblin", targettoremove.CustomTimer.Name);
                isCalled = true;
            };

            var line = "You say, 'PigTimer-30:00-StupidGoblin'";
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_WithSeconds()
        {
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual((30 * 60) + 20, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("StupidGoblin", targettoremove.CustomTimer.Name);
                isCalled = true;
            };

            var line = "You say, 'PigTimer-30:20-StupidGoblin'";
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_WithSeconds_andmorethan60minutes()
        {
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual((90 * 60) + 20, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("StupidGoblin", targettoremove.CustomTimer.Name);
                isCalled = true;
            };
            var line = "You say, 'PigTimer-90:20-StupidGoblin'";
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart1()
        {
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual(30 * 60, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("StupidGoblin", targettoremove.CustomTimer.Name);
                isCalled = true;
            };
            var line = "You say, 'PigTimer-30:00-StupidGoblin'";
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_TestSpaces()
        {
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual(30 * 60, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("StupidGoblin_with_club_near_me", targettoremove.CustomTimer.Name);
                isCalled = true;
            };
            var line = "You say, 'PigTimer-30:00-StupidGoblin_with_club_near_me'";
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test1()
        {
            var line = "You say, 'PigTimer-02'";
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual(2, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("PigTimer-02", targettoremove.CustomTimer.Name);
                isCalled = true;
            };

            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test2()
        {
            var line = "You say, 'PigTimer-02:03'";
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual((2 * 60) + 3, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("PigTimer-02:03", targettoremove.CustomTimer.Name);
                isCalled = true;
            };
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test3()
        {
            var line = "You say, 'PigTimer-02:03:04'";
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual((2 * 3600) + (3 * 60) + 4, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("PigTimer-02:03:04", targettoremove.CustomTimer.Name);
                isCalled = true;
            };

            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test1a()
        {
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual(2, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("xyzzy", targettoremove.CustomTimer.Name);
                isCalled = true;
            };
            var line = "You say, 'PigTimer-02-xyzzy'";
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test2a()
        {
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual((2 * 60) + 3, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("xyzzy", targettoremove.CustomTimer.Name);
                isCalled = true;
            };
            var line = "You say, 'PigTimer-02:03-xyzzy'";
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test3a()
        {
            logEvents.StartTimerEvent += (s, targettoremove) =>
            {
                Assert.IsNotNull(targettoremove);
                Assert.AreEqual((2 * 3600) + (3 * 60) + 4, targettoremove.CustomTimer.DurationInSeconds);
                Assert.AreEqual("xyzzy", targettoremove.CustomTimer.Name);
                isCalled = true;
            };
            var line = "You say, 'PigTimer-02:03:04-xyzzy'";
            logParser.Push(line, DateTime.Now);
            Assert.IsTrue(isCalled);
        }
    }
}
