using EQToolApis.DB;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.Services
{
    public class SQLIndexRebuild
    {
        private readonly EQToolContext dbcontext;
        public SQLIndexRebuild(EQToolContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public void RebuildAll()
        {
            _ = dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQAuctionPlayers] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQitems] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EqToolExceptions] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQTunnelAuctionItems] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQTunnelMessages] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [Players] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }
    }
}
