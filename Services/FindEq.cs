namespace EqTool.Services
{
    public static class FindEq
    {
        public static readonly string BestGuessRootEqPath = string.Empty;
        private class Match
        {
            public DateTime LastModifiedDate { get; set; }
            public string RootPath { get; set; }
            public bool HasCharUiFiles { get; set; }
        }

        static FindEq()
        {
            var possibles = new List<Match>();
            var p99license = File.ReadAllText("p99licensefile.txt");
            var p99licensehash = StringHash.sha256_hash(p99license);

            foreach (var f in DriveInfo.GetDrives().Where(a => a.IsReady && a.DriveType == DriveType.Fixed))
            {
                var files = Directory.EnumerateFiles(f.Name, "eqgame.exe", new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    MaxRecursionDepth = 2,
                    RecurseSubdirectories = true
                }).ToList();

                foreach (var item in files)
                {
                    var root = Path.GetDirectoryName(item);
                    var licensetext = File.ReadAllText(root + "/license.txt");
                    var licensehash = StringHash.sha256_hash(licensetext);
                    if (licensehash == p99licensehash)
                    {
                        var directory = new DirectoryInfo(root);
                        var maxmoddate = directory.GetFiles()
                            .OrderByDescending(a => a.LastWriteTime)
                            .Where(a => a.Name.StartsWith("UI_") && a.Name.EndsWith(".ini"))
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
                                possibles.Add(new Match
                                {
                                    LastModifiedDate = maxmoddate.Value,
                                    RootPath = root,
                                    HasCharUiFiles = false
                                });
                            }
                        }
                        else
                        {
                            possibles.Add(new Match
                            {
                                LastModifiedDate = maxmoddate.Value,
                                RootPath = root,
                                HasCharUiFiles = true
                            });
                        }
                    }
                }
            }
            var rootfolder = possibles.Where(a => a.HasCharUiFiles).OrderByDescending(a => a.LastModifiedDate).Select(a => a.RootPath).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(rootfolder))
            {
                rootfolder = possibles.OrderByDescending(a => a.LastModifiedDate).Select(a => a.RootPath).FirstOrDefault();
            }
            BestGuessRootEqPath = rootfolder;
        }
    }
}
