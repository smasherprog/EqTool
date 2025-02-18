using Autofac;
using EQTool.Services;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class GroupLeaderTests : BaseTestClass
    {

        private readonly LogParser logParser;
        private readonly SettingsWindowViewModel viewModel;

        // ctor
        public GroupLeaderTests()
        {
            logParser = container.Resolve<LogParser>();
            viewModel = container.Resolve<SettingsWindowViewModel>();
        }

        [TestMethod]
        public void TestYouJoin()
        {
            viewModel.GroupLeaderName = string.Empty;
            string line = "You notify Gabes that you agree to join the group.";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual("Gabes", viewModel.GroupLeaderName);
        }

        [TestMethod]
        public void TestYouInvite()
        {
            viewModel.GroupLeaderName = string.Empty;
            string line = "You invite Legendarymonk to join your group.";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual("You", viewModel.GroupLeaderName);
        }

        [TestMethod]
        public void TestLeaderChanged()
        {
            viewModel.GroupLeaderName = string.Empty;
            string line = "Doab is now the leader of your group.";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual("Doab", viewModel.GroupLeaderName);
        }

        [TestMethod]
        public void TestUngroup1()
        {
            viewModel.GroupLeaderName = string.Empty;
            string line = "Your group has been disbanded.";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual("None", viewModel.GroupLeaderName);
        }

        [TestMethod]
        public void TestUngroup2()
        {
            viewModel.GroupLeaderName = string.Empty;
            string line = "You have been removed from the group.";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual("None", viewModel.GroupLeaderName);
        }

        [TestMethod]
        public void TestLogin()
        {
            viewModel.GroupLeaderName = string.Empty;
            string line = "Welcome to EverQuest!";
            logParser.Push(line, DateTime.Now);
            Assert.AreEqual("None", viewModel.GroupLeaderName);
        }


    }
}
