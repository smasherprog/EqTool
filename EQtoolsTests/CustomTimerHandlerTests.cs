using Autofac;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.SpellWindow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class CustomTimerHandlerTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public CustomTimerHandlerTests()
        {
            logParser = container.Resolve<LogParser>();
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
        }

        [TestMethod]
        public void GetCustomTimerStart()
        {
            var line = "You say, 'PigTimer-30:00-StupidGoblin'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "StupidGoblin") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == 30 * 60);
        }

        [TestMethod]
        public void GetCustomTimerStart_WithSeconds()
        {
            var line = "You say, 'PigTimer-30:20-StupidGoblin'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "StupidGoblin") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == (30 * 60) + 20);
        }

        [TestMethod]
        public void GetCustomTimerStart_WithSeconds_andmorethan60minutes()
        {
            var line = "You say, 'PigTimer-90:20-StupidGoblin'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "StupidGoblin") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == (90 * 60) + 20);
        }

        [TestMethod]
        public void GetCustomTimerStart1()
        {
            var line = "You say, 'PigTimer-30:00-StupidGoblin'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "StupidGoblin") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == 30 * 60);
        }

        [TestMethod]
        public void GetCustomTimerStart_TestSpaces()
        {
            var line = "You say, 'PigTimer-30:00-StupidGoblin_with_club_near_me'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "StupidGoblin_with_club_near_me") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == 30 * 60);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test1()
        {
            var line = "You say, 'PigTimer-02'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "PigTimer-02") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == 2);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test2()
        {
            var line = "You say, 'PigTimer-02:03'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "PigTimer-02:03") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == (2 * 60) + 3);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test3()
        {
            var line = "You say, 'PigTimer-02:03:04'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "PigTimer-02:03:04") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == (2 * 3600) + (3 * 60) + 4);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test1a()
        {
            var line = "You say, 'PigTimer-02-xyzzy'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "xyzzy") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == 2);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test2a()
        {
            var line = "You say, 'PigTimer-02:03-xyzzy'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "xyzzy") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == (2 * 60) + 3);
        }

        [TestMethod]
        public void GetCustomTimerStart_Test3a()
        {
            var line = "You say, 'PigTimer-02:03:04-xyzzy'";
            logParser.Push(line, DateTime.Now);
            var timer = spellWindowViewModel.SpellList.FirstOrDefault(a => a.SpellViewModelType == SpellViewModelType.Timer && a.Id == "xyzzy") as TimerViewModel;
            Assert.IsNotNull(timer);
            Assert.IsTrue(timer.TotalDuration.TotalSeconds == (2 * 3600) + (3 * 60) + 4);
        }
    }
}
