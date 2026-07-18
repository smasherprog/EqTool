using EQToolShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services
{
    // Flags EverQuest / Pigparse install locations that are known to break log and UI
    // file access:
    //   - Program Files: Windows redirects writes to the per-user VirtualStore.
    //   - Desktop / OneDrive: permission redirection and cloud sync can lock, move, or
    //     offload files out from under the tool.
    //   - Pigparse installed inside the EverQuest folder: EQ patches/updates can clobber it.
    // Each location causes a different failure, so the warnings are reported separately.
    public static class InstallPathChecker
    {
        private const StringComparison OIC = StringComparison.OrdinalIgnoreCase;

        // Warning text for the EverQuest install location, or empty if it looks fine.
        public static string GetEqPathWarning(string eqDirectory)
        {
            if (string.IsNullOrWhiteSpace(eqDirectory))
            {
                return string.Empty;
            }
            var problems = LocationProblems(eqDirectory, "EverQuest");
            return problems.Count == 0
                ? string.Empty
                : "EverQuest install location warning:\n" + string.Join("\n", problems);
        }

        // Warning text for the Pigparse install location, or empty if it looks fine. Also
        // flags Pigparse living inside the EverQuest folder.
        public static string GetEqToolPathWarning(string eqDirectory)
        {
            string toolDir;
            try
            {
                toolDir = Paths.ExecutableDirectory();
            }
            catch
            {
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(toolDir))
            {
                return string.Empty;
            }

            var problems = LocationProblems(toolDir, "Pigparse");
            if (!string.IsNullOrWhiteSpace(eqDirectory) && IsInside(toolDir, eqDirectory))
            {
                problems.Add("- Pigparse is installed inside your EverQuest folder. Install it in its own separate folder so EQ patches/updates don't remove or interfere with it.");
            }
            return problems.Count == 0
                ? string.Empty
                : "Pigparse install location warning:\n" + string.Join("\n", problems);
        }

        private static List<string> LocationProblems(string path, string app)
        {
            var list = new List<string>();
            if (IsUnderProgramFiles(path))
            {
                list.Add($"- {app} is under \"Program Files\". Windows redirects its files to the VirtualStore, which can break log and UI file access. Use a path like C:\\{app}.");
            }
            if (IsUnderDesktop(path))
            {
                list.Add($"- {app} is on your Desktop. Files there can hit permission/redirection issues. Use a path like C:\\{app}.");
            }
            if (IsUnderOneDrive(path))
            {
                list.Add($"- {app} is inside OneDrive. OneDrive can lock, move, or cloud-offload files and break access. Move it outside OneDrive (e.g. C:\\{app}).");
            }
            return list;
        }

        public static bool IsUnderProgramFiles(string path)
        {
            // Matches both "Program Files" and "Program Files (x86)".
            return !string.IsNullOrWhiteSpace(path) &&
                Segments(path).Any(s => s.StartsWith("Program Files", OIC));
        }

        public static bool IsUnderDesktop(string path)
        {
            if (StartsWithFolder(path, Environment.SpecialFolder.Desktop) ||
                StartsWithFolder(path, Environment.SpecialFolder.DesktopDirectory))
            {
                return true;
            }
            return !string.IsNullOrWhiteSpace(path) && Segments(path).Any(s => s.Equals("Desktop", OIC));
        }

        public static bool IsUnderOneDrive(string path)
        {
            foreach (var v in new[] { "OneDrive", "OneDriveConsumer", "OneDriveCommercial" })
            {
                var od = Environment.GetEnvironmentVariable(v);
                if (!string.IsNullOrWhiteSpace(od) && IsInside(path, od))
                {
                    return true;
                }
            }
            // Covers "OneDrive" and business folders like "OneDrive - Contoso".
            return !string.IsNullOrWhiteSpace(path) && Segments(path).Any(s => s.StartsWith("OneDrive", OIC));
        }

        // True if 'path' is the same folder as, or nested inside, 'root'.
        public static bool IsInside(string path, string root)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(root))
            {
                return false;
            }
            var p = Normalize(path);
            var r = Normalize(root);
            return p.Equals(r, OIC) || p.StartsWith(r + "\\", OIC);
        }

        private static bool StartsWithFolder(string path, Environment.SpecialFolder folder)
        {
            string f;
            try
            {
                f = Environment.GetFolderPath(folder);
            }
            catch
            {
                return false;
            }
            return IsInside(path, f);
        }

        private static string Normalize(string path)
        {
            return path.Replace('/', '\\').TrimEnd('\\');
        }

        private static string[] Segments(string path)
        {
            return path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
