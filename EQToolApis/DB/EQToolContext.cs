
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
        public DbSet<EQAuctionPlayer> EQAuctionPlayers { get; set; }

        public DbSet<EQitem> EQitems { get; set; }

        public DbSet<EQTunnelAuctionItem> EQTunnelAuctionItems { get; set; }

        public DbSet<EQTunnelMessage> EQTunnelMessages { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<ServerMessage> ServerMessages { get; set; }

        public DbSet<EqToolException> EqToolExceptions { get; set; }
    }
}