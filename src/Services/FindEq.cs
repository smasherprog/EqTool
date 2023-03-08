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
        private class Match : FindEQData
        {
            public DateTime LastModifiedDate { get; set; }
            public bool HasCharUiFiles { get; set; }
        }

        public class FindEQData
        {
            public string EqBaseLocation { get; set; }

            public string EQlogLocation { get; set; }
        }

        public FindEQData LoadEQPath()
        {
            var possibles = new ConcurrentQueue<Match>();
            var drives = DriveInfo.GetDrives().Where(a => a.IsReady && a.DriveType == DriveType.Fixed);
            var tasks = new List<Task>();
            foreach (var f in drives.Take(1))
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
                            var dirdata = GetUIFiles(root)
                                                        .OrderByDescending(a => a.LastWriteTime)
                                                        .Select(a => new { a.LastWriteTime, a.FullName })
                                                        .FirstOrDefault();
                            if (dirdata != null)
                            {
                                var directory = new DirectoryInfo(root);
                                var maxmoddate = directory.GetFiles()
                                .OrderByDescending(a => a.LastWriteTime)
                                .Select(a => (DateTime?)a.LastWriteTime)
                                .FirstOrDefault();
                                if (maxmoddate.HasValue)
                                {
                                    possibles.Enqueue(new Match
                                    {
                                        LastModifiedDate = maxmoddate.Value,
                                        EqBaseLocation = root,
                                        HasCharUiFiles = false
                                    });
                                }
                            }
                            else
                            {
                                possibles.Enqueue(new Match
                                {
                                    LastModifiedDate = dirdata.LastWriteTime,
                                    EqBaseLocation = root,
                                    HasCharUiFiles = true
                                });
                            }
                        }
                    }
                }));
            }
            var allran = Task.WaitAll(tasks.ToArray(), 1000 * 5);
            var rootfolder = possibles.Where(a => a.HasCharUiFiles).OrderByDescending(a => a.LastModifiedDate).FirstOrDefault();
            if (rootfolder == null)
            {
                rootfolder = possibles.OrderByDescending(a => a.LastModifiedDate).FirstOrDefault();
            }
            var logifles = GetLogFileLocation(new FindEQData
            {
                EqBaseLocation = rootfolder?.EqBaseLocation,
                EQlogLocation = rootfolder?.EQlogLocation,
            });
            rootfolder.EQlogLocation = logifles.Location;
            return rootfolder;
        }

        public static bool IsProject1999Folder(string rootfolder)
        {
            if (string.IsNullOrWhiteSpace(rootfolder))
            {
                return false;
            }

            try
            {
                var licensetext = File.ReadAllText(rootfolder + "/license.txt");
                if (licensetext.Contains("Project 1999"))
                {
                    return true;
                }
            }
            catch { }

            return false;
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

        private static string GetVirtualStoreLocation(string path)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\VirtualStore\\" + path.Substring(3);
        }


        public static FileInfo[] GetUIFiles(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return new FileInfo[0];
            }

            try
            {
                var directory = new DirectoryInfo(root);
                var files = directory.GetFiles("UI_*.ini", SearchOption.TopDirectoryOnly);
                if (files.Any())
                {
                    return files;
                }
            }
            catch { }

            try
            {
                root = GetVirtualStoreLocation(root);
                var directory = new DirectoryInfo(root);
                var files = directory.GetFiles("UI_*.ini", SearchOption.TopDirectoryOnly);
                if (files.Any())
                {
                    return files;
                }
            }
            catch { }

            return new FileInfo[0];
        }

        public class LogFileInfo
        {
            public string Location { get; set; }
            public bool Found { get; set; }
        }

        public static LogFileInfo GetLogFileLocation(FindEQData data)
        {
            var ret = new LogFileInfo { Found = false };
            try
            {
                if (!string.IsNullOrWhiteSpace(data.EQlogLocation))
                {
                    ret.Found = Directory.EnumerateFiles(data.EQlogLocation, "eqlog*.txt", SearchOption.TopDirectoryOnly).Any();
                }
            }
            catch { }
            if (ret.Found)
            {
                ret.Location = data.EQlogLocation;
                return ret;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(data.EqBaseLocation))
                {
                    ret.Found = Directory.EnumerateFiles(data.EqBaseLocation + "/Logs/", "eqlog*.txt", SearchOption.TopDirectoryOnly).Any();
                }
            }
            catch { }
            if (ret.Found)
            {
                ret.Location = data.EqBaseLocation + "/Logs/";
                return ret;
            }

            var root = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(data.EqBaseLocation))
                {
                    root = GetVirtualStoreLocation(data.EqBaseLocation) + "/Logs/";
                    ret.Found = Directory.EnumerateFiles(root, "eqlog*.txt", SearchOption.TopDirectoryOnly).Any();
                }
            }
            catch { }
            if (ret.Found)
            {
                ret.Location = root;
                return ret;
            }



            return ret;
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
                catch
                { }
                list.AddRange(Directory.EnumerateFiles(root, pathtomatch));
            }
            catch
            { }

            return list;
        }
    }
}
