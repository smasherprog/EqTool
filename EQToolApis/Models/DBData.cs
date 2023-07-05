using EQToolShared.Enums;

namespace EQToolApis.Models
{
    public class ServerDBData
    {
        public long? OrderByDescendingDiscordMessageId { get; set; }

        public long? OrderByDiscordMessageId { get; set; }

        public int TotalEQTunnelMessages;

        public int TotalEQTunnelAuctionItems;

        public DateTimeOffset RecentImportTimeStamp { get; set; }

        public DateTimeOffset OldestImportTimeStamp { get; set; }
    }

    public class DBData
    {
        public int TotalEQAuctionPlayers;

        public int TotalUniqueItems;

        public ServerDBData[] ServerData { get; set; } = new ServerDBData[(int)(Servers.Blue + 1)];
    }

    public class AuctionPlayer
    {
        public int EQAuctionPlayerId { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    public class PlayerCache
    {
        public ReaderWriterLockSlim PlayersLock = new();
        public Dictionary<int, AuctionPlayer> Players = new();
    }

    public class NoteableNPC
    {
        public DateTimeOffset? LastSeen { get; set; }
        public DateTimeOffset? LastDeath { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EQNotableNPCId { get; set; }
    }


    public class NoteableNPCZone
    {
        public Dictionary<string, List<NoteableNPC>> Zones = new();
    }

    public class NoteableNPCCache
    {
        public NoteableNPCCache()
        {
            ServerData = new NoteableNPCZone[(int)Servers.Red + 1];
            ServerData[(int)Servers.Green] = new NoteableNPCZone();
            ServerData[(int)Servers.Blue] = new NoteableNPCZone();
            ServerData[(int)Servers.Red] = new NoteableNPCZone();
        }
        public NoteableNPCZone[] ServerData { get; set; } = new NoteableNPCZone[(int)Servers.Red + 1];
    }
}
