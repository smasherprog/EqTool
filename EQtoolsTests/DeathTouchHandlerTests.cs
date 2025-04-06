using Autofac;
using EQTool.Services;
using EQTool.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EQtoolsTests
{
    [TestClass]
    public class DeathTouchHandlerTests : BaseTestClass
    {
        private readonly LogParser logParser;
        private readonly SpellWindowViewModel spellWindowViewModel;

        public DeathTouchHandlerTests()
        {
            spellWindowViewModel = container.Resolve<SpellWindowViewModel>();
            logParser = container.Resolve<LogParser>();
        }

        [TestMethod]
        public void Test1()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNull(dteffect);
            logParser.Push("Dread says 'TINIALITA'", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNotNull(dteffect);
            Assert.IsTrue(dteffect.Name == "--DT-- 'TINIALITA'");
        }

        [TestMethod]
        public void Test2()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNull(dteffect);
            logParser.Push("Dread says, 'You will not evade me Silvose!'", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNull(dteffect);
        }

        [TestMethod]
        public void Test3()
        {
            var dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNull(dteffect);
            logParser.Push("Fright says 'TINIALITA'", DateTime.Now);
            dteffect = spellWindowViewModel.SpellList.FirstOrDefault(a => a.Name.StartsWith("--DT--"));
            Assert.IsNotNull(dteffect);
        }
    }
}
