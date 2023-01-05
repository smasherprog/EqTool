using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EQTool.Services
{
    public class FindEq
    {
        private class Match
        {
            public DateTime LastModifiedDate { get; set; }
            public string RootPath { get; set; }
            public bool HasCharUiFiles { get; set; }
        }

        public string LoadEQPath()
        {
            var possibles = new ConcurrentQueue<Match>();
            var drives = DriveInfo.GetDrives().Where(a => a.IsReady && a.DriveType == DriveType.Fixed);
            var tasks = new List<Task>();
            foreach (var f in drives)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var files = GetFilesAndFolders(f.Name, "eqgame.exe", 2);

                    foreach (var item in files)
                    {
                        var root = Path.GetDirectoryName(item);
                        var licensetext = File.ReadAllText(root + "/license.txt");
                        if (licensetext.Contains("Project 1999"))
                        {
                            var directory = new DirectoryInfo(root);
                            var maxmoddate = directory.GetFiles("UI_*.ini")
                                .OrderByDescending(a => a.LastWriteTime) 
                                .Select(a => (DateTime?)a.LastWriteTime)
                                .FirstOrDefault();
                            if (!maxmoddate.HasValue)
                            {
                                maxmoddate = directory.GetFiles()
                                .OrderByDescending(a => a.LastWriteTime)
                                .Select(a => (DateTime?)a.LastWriteTime)
                                .FirstOrDefault();
                                if (maxmoddate.HasValue)
                                {
                                    possibles.Enqueue(new Match
                                    {
                                        LastModifiedDate = maxmoddate.Value,
                                        RootPath = root,
                                        HasCharUiFiles = false
                                    });
                                }
                            }
                            else
                            {
                                possibles.Enqueue(new Match
                                {
                                    LastModifiedDate = maxmoddate.Value,
                                    RootPath = root,
                                    HasCharUiFiles = true
                                });
                            }
                        }
                    }
                }));
            }
            var allran = Task.WaitAll(tasks.ToArray(), 1000 * 5);
            var rootfolder = possibles.Where(a => a.HasCharUiFiles).OrderByDescending(a => a.LastModifiedDate).Select(a => a.RootPath).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(rootfolder))
            {
                rootfolder = possibles.OrderByDescending(a => a.LastModifiedDate).Select(a => a.RootPath).FirstOrDefault();
            }
            return rootfolder;
        }

        public static bool? TryCheckLoggingEnabled(string eqdir)
        {
            try
            {
                var data = File.ReadAllLines(eqdir + "/eqclient.ini");
                foreach (var item in data)
                {
                    var line = item.ToLower().Trim().Replace(" ", string.Empty);
                    if (line.StartsWith("log="))
                    {
                        return line.Contains("true");
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        public static bool IsValid(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return false;
            }
            try
            {
                var directory = new DirectoryInfo(root);
                var maxmoddate = directory.GetFiles("UI_*.ini")
                    .OrderByDescending(a => a.LastWriteTime)
                    .Select(a => (DateTime?)a.LastWriteTime)
                    .FirstOrDefault();
                return maxmoddate.HasValue;
            }
            catch { }

            return false;
        }

        private static List<string> GetFilesAndFolders(string root, string pathtomatch, int depth)
        {
            var list = new List<string>();
            try
            {
                try
                {
                    foreach (var directory in Directory.EnumerateDirectories(root).Where(a => !a.Contains("$")))
                    {
                        if (depth > 0)
                        {
                            list.AddRange(GetFilesAndFolders(directory, pathtomatch, depth - 1));
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                { }
                list.AddRange(Directory.EnumerateFiles(root, pathtomatch));
            }
            catch (UnauthorizedAccessException)
            { }

            return list;
        }
    }
}
