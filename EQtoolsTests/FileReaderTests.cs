using Autofac;
using EQTool.Services;
using EQTool.Services.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using EQToolShared.Extensions;

namespace EQtoolsTests
{
    [TestClass]
    public class FileReaderTests : BaseTestClass
    {
        private readonly IFileReader fileReader;
        private readonly LogEvents logEvents;
        private readonly string FilePath = Paths.Combine(Directory.GetParent(Paths.ExecutableDirectory()).Parent.FullName, "LogFiles");

        public FileReaderTests()
        {
            fileReader = container.Resolve<IFileReader>();
            logEvents = container.Resolve<LogEvents>();
        }

        [TestMethod]
        public void HappyPath()
        {
            var lines = fileReader.ReadNext(Paths.Combine(FilePath, "log1.txt"));
            Assert.IsNotNull(lines);
            Assert.IsTrue(lines.Any());
            Assert.IsNotNull(lines.FirstOrDefault(a => a.Contains("Welcome to EverQuest!")));
        }
    }
}
