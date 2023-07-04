
using EQToolApis.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.DB
{
    public class EQToolContext : DbContext
    {
        public EQToolContext(DbContextOptions options)
            : base(options)
        {

        }

        public DbSet<APILog> APILogs { get; set; }

        public DbSet<IPBan> IPBans { get; set; }

        public DbSet<EQZone> EQZones { get; set; }

        public DbSet<EQDeath> EQDeaths { get; set; }

        public DbSet<EQAuctionPlayer> EQAuctionPlayers { get; set; }

        public DbSet<EQitem> EQitems { get; set; }

        public DbSet<EQNotableNPC> EQNotableNPCs { get; set; }

        public DbSet<EQNotableActivity> EQNotableActivities { get; set; }

        public DbSet<EQTunnelAuctionItem> EQTunnelAuctionItems { get; set; }

        public DbSet<EQTunnelMessage> EQTunnelMessages { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<ServerMessage> ServerMessages { get; set; }

        public DbSet<EqToolException> EqToolExceptions { get; set; }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            //     _ = configurationBuilder.Properties<string>().AreUnicode(false);
        }
    }
}