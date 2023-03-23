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
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQAuctionPlayers] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQitems] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EqToolExceptions] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQTunnelAuctionItems] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQTunnelMessages] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [Players] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }

        //        public void FixOutlierData()
        //        {
        //            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
        //            _ = dbcontext.Database.ExecuteSqlRaw(@"update eai
        //set eai.AuctionPrice = null
        //  from EQTunnelAuctionItems eai
        //  join EQitems i on eai.EQitemId = i.EQitemId
        //  where eai.AuctionPrice is NOT null AND eai.AuctionPrice < [TotalWTSLast30DaysAverage]*.1");
        //        }
    }
}
