using EQToolShared.Enums;
using System;
using System.IO;

namespace EQToolShared
{
    public class UIFileNameInfo
    {
        public string PlayerName { get; set; }
        public Servers Server { get; set; }
        // True for the "UI_<name>_<server>.ini" layout file, false for the
        // companion "<name>_<server>.ini".
        public bool IsUiLayoutFile { get; set; }
    }

    // Parses the EverQuest per-character config-file pair. The two files always
    // share the "<name>_<serverToken>.ini" suffix; the UI layout file adds a
    // "UI_" prefix. Server tokens mirror the mapping used elsewhere in the app
    // (see SettingsGeneral.SelectMasterUIFile / ActivePlayerInfo.GetInfoFromString):
    //   P1999Green -> Green, P1999Blue -> Blue, P1999PVP -> Red.
    // Anything that does not end in a recognized token (e.g. Quarm) is not a pair
    // file and is ignored by the sync.
    public static class UIFileName
    {
        public static bool IsUiPairFile(string fileName)
        {
            return TryParse(fileName, out _);
        }

        public static bool TryParse(string fileName, out UIFileNameInfo info)
        {
            info = null;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            // Accept a full path or a bare file name.
            var name = Path.GetFileName(fileName);
            if (!name.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var withoutExt = name.Substring(0, name.Length - ".ini".Length);
            var isUi = false;
            if (withoutExt.StartsWith("UI_", StringComparison.OrdinalIgnoreCase))
            {
                isUi = true;
                withoutExt = withoutExt.Substring("UI_".Length);
            }

            // "<name>_<serverToken>" - character names never contain '_', so the
            // last '_' separates the name from the server token.
            var lastUnderscore = withoutExt.LastIndexOf('_');
            if (lastUnderscore <= 0 || lastUnderscore >= withoutExt.Length - 1)
            {
                return false;
            }

            var playerName = withoutExt.Substring(0, lastUnderscore);
            var serverToken = withoutExt.Substring(lastUnderscore + 1);

            Servers server;
            if (string.Equals(serverToken, "P1999Green", StringComparison.OrdinalIgnoreCase))
            {
                server = Servers.Green;
            }
            else if (string.Equals(serverToken, "P1999Blue", StringComparison.OrdinalIgnoreCase))
            {
                server = Servers.Blue;
            }
            else if (string.Equals(serverToken, "P1999PVP", StringComparison.OrdinalIgnoreCase))
            {
                server = Servers.Red;
            }
            else
            {
                return false;
            }

            info = new UIFileNameInfo
            {
                PlayerName = playerName,
                Server = server,
                IsUiLayoutFile = isUi
            };
            return true;
        }
    }
}
