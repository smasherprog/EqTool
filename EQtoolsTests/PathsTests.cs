using System;
using System.IO;
using EQToolShared.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EQtoolsTests
{
    [TestClass]
    public class PathsTests
    {
        [TestMethod]
        [DataRow("C:\\Everquest", "eqclient.ini")]
        [DataRow("C:\\Everquest\\", "eqclient.ini")]
        [DataRow("C:\\Everquest/", "eqclient.ini")]
        [DataRow("C:\\Everquest", "/eqclient.ini")]
        [DataRow("C:\\Everquest\\", "/eqclient.ini")]
        [DataRow("C:\\Everquest/", "/eqclient.ini")]
        [DataRow("C:\\Everquest", "\\eqclient.ini")]
        [DataRow("C:\\Everquest\\", "\\eqclient.ini")]
        [DataRow("C:\\Everquest/", "\\eqclient.ini")]
        public void SimplePathCombineTests(string directory, string file)
        {
            var combined = Paths.Combine(directory, file);
            
            Console.WriteLine(combined);
            Assert.AreEqual($"C:{Path.DirectorySeparatorChar}Everquest{Path.DirectorySeparatorChar}eqclient.ini", combined);
        }
        
        [TestMethod]
        [DataRow("C:\\Program Files (x86)\\Everquest", "eqclient.ini")]
        [DataRow("C:\\Program Files (x86)\\Everquest\\", "eqclient.ini")]
        [DataRow("C:\\Program Files (x86)\\Everquest/", "eqclient.ini")]
        [DataRow("C:\\Program Files (x86)\\Everquest", "/eqclient.ini")]
        [DataRow("C:\\Program Files (x86)\\Everquest\\", "/eqclient.ini")]
        [DataRow("C:\\Program Files (x86)\\Everquest/", "/eqclient.ini")]
        [DataRow("C:\\Program Files (x86)\\Everquest", "\\eqclient.ini")]
        [DataRow("C:\\Program Files (x86)\\Everquest\\", "\\eqclient.ini")]
        [DataRow("C:\\Program Files (x86)\\Everquest/", "\\eqclient.ini")]
        public void ProgramPathCombineTests(string directory, string file)
        {
            var combined = Paths.Combine(directory, file);
            
            Console.WriteLine(combined);
            Assert.AreEqual($"C:{Path.DirectorySeparatorChar}Program Files (x86){Path.DirectorySeparatorChar}Everquest{Path.DirectorySeparatorChar}eqclient.ini", combined);
        }
        
        [TestMethod]
        [DataRow("C:\\PigParse", "NewVersion")]
        [DataRow("C:\\PigParse\\", "NewVersion")]
        [DataRow("C:\\PigParse/", "NewVersion")]
        [DataRow("C:\\PigParse", "/NewVersion")]
        [DataRow("C:\\PigParse\\", "/NewVersion")]
        [DataRow("C:\\PigParse/", "/NewVersion")]
        [DataRow("C:\\PigParse", "\\NewVersion")]
        [DataRow("C:\\PigParse\\", "\\NewVersion")]
        [DataRow("C:\\PigParse/", "\\NewVersion")]
        public void DirectoryCombineTests(string directory, string otherDirectory)
        {
            var combined = Paths.Combine(directory, otherDirectory);
            
            Console.WriteLine(combined);
            Assert.AreEqual($"C:{Path.DirectorySeparatorChar}PigParse{Path.DirectorySeparatorChar}NewVersion", combined);
        }

        [TestMethod]
        [DataRow("SpellsCacheOrange_1")]
        [DataRow("mappdata.data")]
        [DataRow("settings.json")]
        public void InExecutableDirectory(string fileOrDir)
        {
            var inExecutableDir = Paths.InExecutableDirectory(fileOrDir);
            var executableDir = Paths.ExecutableDirectory();
            
            Console.WriteLine(inExecutableDir);
            var expectedPath = executableDir + Path.DirectorySeparatorChar + fileOrDir;
            Assert.AreEqual(expectedPath, inExecutableDir);
        }
    }
}
