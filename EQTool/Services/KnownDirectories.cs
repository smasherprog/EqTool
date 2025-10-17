using System.Diagnostics;
using System.IO;

namespace EQTool.Services
{
    public static class KnownDirectories
    {
        public static string GetExecutableDirectory()
        {
            var exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrWhiteSpace(exeFilePath))
            {
                // This directory will be incorrect if the user is launching from a batch file or other external process, but it's better than nothing.
                return Directory.GetCurrentDirectory();
            }

            return Path.GetDirectoryName(exeFilePath) ?? Directory.GetCurrentDirectory();
        }
    }
}
