using Autofac;
using EQTool.Services;
using EQtoolsTests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EQtoolsTests
{
    [TestClass]
    public class InvisFadingTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly TextToSpeachFake textToSpeach;
        public InvisFadingTests()
        { 
            logParser = container.Resolve<LogParser>();
            textToSpeach = container.Resolve<ITextToSpeach>() as TextToSpeachFake;
        }

        [TestMethod]
        public void Test()
        { 
          this.player.Player.InvisFadingAudio = true;
            var called = false;
            textToSpeach.TextToSpeachCallback = (text) =>
            {
                Assert.AreEqual("Invisability Fading.", text);
                called = true;
            };
            logParser.Push("You feel yourself starting to appear.", DateTime.Now);
     
            Assert.IsTrue(called); 
        } 
    }
}
