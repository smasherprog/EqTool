
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
        public DbSet<Player> Players { get; set; }

        public DbSet<EqToolException> EqToolExceptions { get; set; }
    }
}