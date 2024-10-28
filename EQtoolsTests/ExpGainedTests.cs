using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Autofac;
using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;


namespace EQtoolsTests
{
    [TestClass]
    public class ExpGainedTests : BaseTestClass
    {
        private readonly ExpGainedParser parser;

        public ExpGainedTests()
        {
            parser = container.Resolve<ExpGainedParser>();
        }

        [TestMethod]
        public void YouGainExp()
        {
            //You gain experience!!
            var now = DateTime.Now;
            var message = "You gain experience!!";
            var match = parser.Match(message, now, 0);

            Assert.IsNotNull(match);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

        [TestMethod]
        public void YouGainPartExp()
        {
            //You gain party experience!!
            var now = DateTime.Now;
            var message = "You gain party experience!!";
            var match = parser.Match(message, now, 0);

            Assert.IsNotNull(match);
            Assert.AreEqual(now, match.TimeStamp);
            Assert.AreEqual(message, match.Line);
        }

    }
}
