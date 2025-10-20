using System.IO;

namespace EQToolShared.Extensions
{
    public static class Paths
    {
        public static string ExecutableDirectory()
        {
            var exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrWhiteSpace(exeFilePath))
            {
                // This directory will be incorrect if the user is launching from a batch file or other external process, but it's better than nothing.
                return Directory.GetCurrentDirectory();
            }

            return Path.GetDirectoryName(exeFilePath) ?? Directory.GetCurrentDirectory();
        }
        
        public static string InExecutableDirectory(string fileName)
            => Combine(ExecutableDirectory(), fileName);
        
        public static string Combine(string path1, string path2)
        {
            if (path1 == null)
            {
                return path2;
            }

            if (path2 == null)
            {
                return path1;
            }

            return path1.Trim().TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar
                + path2.Trim().TrimStart(Path.DirectorySeparatorChar).TrimStart(Path.AltDirectorySeparatorChar);
        }

        public static string Combine(string path1, string path2, string path3)
            => Combine(Combine(path1, path2), path3);
    }
}
