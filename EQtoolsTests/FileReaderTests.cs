using Autofac;
using EQTool.Services;
using EQTool.Services.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
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
        public void OldLoginIsNotReplayed()
        {
            var lines = fileReader.ReadNext(Paths.Combine(FilePath, "log1.txt"));
            Assert.IsNotNull(lines);
            Assert.AreEqual(0, lines.Count);
        }

        [TestMethod]
        public void OnlyLinesFromLast15SecondsAreReplayed()
        {
            var path = Path.GetTempFileName();
            try
            {
                var now = DateTime.Now;
                File.WriteAllLines(path, new[]
                {
                    LogLine(now.AddMinutes(-5), "Welcome to EverQuest!"),
                    LogLine(now.AddMinutes(-5), "You have entered East Commonlands."),
                    LogLine(now.AddSeconds(-5), "You have entered The Wakening Lands."),
                    LogLine(now.AddSeconds(-4), "You land.")
                });

                var lines = new FileReader().ReadNext(path);
                Assert.AreEqual(1, lines.Count);
                Assert.AreEqual(LogLine(now.AddSeconds(-5), "You have entered The Wakening Lands."), lines[0]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void QuotedLoginPhrasesAreNotMatched()
        {
            var path = Path.GetTempFileName();
            try
            {
                var now = DateTime.Now;
                File.WriteAllLines(path, new[]
                {
                    LogLine(now, "Welcome to EverQuest!"),
                    LogLine(now, "You have entered Kael Drakkel."),
                    LogLine(now, "GUILD MOTD: Welcome to EverQuest! Raid at 8pm"),
                    LogLine(now, "Bob tells the guild, 'You have entered the danger zone'")
                });

                var lines = new FileReader().ReadNext(path);
                CollectionAssert.AreEqual(new[]
                {
                    LogLine(now, "Welcome to EverQuest!"),
                    LogLine(now, "You have entered Kael Drakkel."),
                    LogLine(now, "GUILD MOTD: Welcome to EverQuest! Raid at 8pm")
                }, lines);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void PartialLineDuringRewindIsReadOnceComplete()
        {
            var path = Path.GetTempFileName();
            try
            {
                var now = DateTime.Now;
                var zoneLine = LogLine(now, "You have entered Nektulos Forest.");
                File.WriteAllText(path, LogLine(now, "Welcome to EverQuest!") + "\r\n" + zoneLine.Substring(0, zoneLine.Length - 11));
                var reader = new FileReader();

                var lines = reader.ReadNext(path);
                Assert.AreEqual(1, lines.Count);
                Assert.AreEqual(LogLine(now, "Welcome to EverQuest!"), lines[0]);

                File.AppendAllText(path, zoneLine.Substring(zoneLine.Length - 11) + "\r\n");
                lines = reader.ReadNext(path);
                Assert.AreEqual(1, lines.Count);
                Assert.AreEqual(zoneLine, lines[0]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void FailedReadStillRewinds()
        {
            var path = Path.GetTempFileName();
            try
            {
                var now = DateTime.Now;
                File.WriteAllLines(path, new[] { LogLine(now, "Welcome to EverQuest!") });
                var reader = new FileReader();

                using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    try
                    {
                        _ = reader.ReadNext(path);
                        Assert.Fail("The locked file should not open.");
                    }
                    catch (IOException)
                    {
                    }
                }

                var lines = reader.ReadNext(path);
                Assert.AreEqual(1, lines.Count);
                Assert.AreEqual(LogLine(now, "Welcome to EverQuest!"), lines[0]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static string LogLine(DateTime time, string message)
        {
            return "[" + time.ToString("ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture) + "] " + message;
        }

        [TestMethod]
        public void PartialLineIsReadOnceComplete()
        {
            var path = Path.GetTempFileName();
            try
            {
                var reader = new FileReader();
                File.WriteAllText(path, "[Sun Oct 11 14:23:40 2026] Welcome to EverQuest!\r\n");
                _ = reader.ReadNext(path);

                File.AppendAllText(path, "[Sun Oct 11 14:23:45 2026] You have ent");
                Assert.AreEqual(0, reader.ReadNext(path).Count);

                File.AppendAllText(path, "ered Nektulos Forest.\r\n");
                var lines = reader.ReadNext(path);
                Assert.AreEqual(1, lines.Count);
                Assert.AreEqual("[Sun Oct 11 14:23:45 2026] You have entered Nektulos Forest.", lines[0]);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
