namespace EQToolApis.DB.Models
{
    public class ServerDBData
    {
        public long? OrderByDescendingDiscordMessageId { get; set; }

        public long? OrderByDiscordMessageId { get; set; }
    }

    public class DBData
    {
        public ServerDBData[] ServerData { get; set; } = new ServerDBData[(int)(Servers.Blue + 1)];
    }
}
