using EQToolShared.Enums;
using System;
using System.Collections.Generic;

namespace EQToolShared.APIModels.UIFileControllerModels
{
    // A single EverQuest UI configuration file (one member of the per-character
    // pair: "UI_<name>_<server>.ini" and "<name>_<server>.ini"). Contents is the
    // plain .ini text.
    public class UIFileUploadRequest
    {
        public string FileName { get; set; } = string.Empty;   // e.g. "UI_Pigy_P1999Green.ini"
        public string PlayerName { get; set; } = string.Empty;  // e.g. "Pigy"
        public Servers Server { get; set; } = Servers.Green;
        public DateTime LastModifiedUtc { get; set; }           // local file LastWriteTimeUtc
        public string Contents { get; set; } = string.Empty;
    }

    // Metadata only (no bytes) - powers the startup reconcile and the management tab.
    public class UIFileMetadata
    {
        public string FileName { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public Servers Server { get; set; } = Servers.Green;
        public DateTime LastModifiedUtc { get; set; }
    }

    public class UIFileDownloadResponse
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime LastModifiedUtc { get; set; }
        public string Contents { get; set; } = string.Empty;
    }
}
