using EQToolApi.DB.Models;
using System.Data.Entity;

namespace EQToolApi.DB
{
    public class EQToolContext : DbContext
    {
        public EQToolContext() : base("eqtooldb") { }
        public DbSet<Player> Players { get; set; }
    }
}