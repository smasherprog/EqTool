using EQToolApis.DB;
using Hangfire;
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

        [AutomaticRetry(Attempts = 0)]
        public void RebuildEQAuctionPlayers()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(20));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQAuctionPlayersV2] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }

        [AutomaticRetry(Attempts = 0)]
        public void RebuildEQitems()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(20));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EqToolExceptions] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQitemsV2] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }

        [AutomaticRetry(Attempts = 0)]
        public void RebuildEQTunnelAuctionItems()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQTunnelAuctionItemsV2] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }

        [AutomaticRetry(Attempts = 0)]
        public void RebuildEQTunnelAuctionEQTunnelMessages()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(20));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQTunnelMessagesV2] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [Players] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }

        [AutomaticRetry(Attempts = 0)]
        public void ItemDupFix()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            _ = dbcontext.Database.ExecuteSqlRaw(@"update eqautionitem
set eqautionitem.EQitemId = dups.DupItemId
from EQTunnelAuctionItems eqautionitem 
join EQitems eqitem on eqautionitem.EQitemId = eqitem.EQitemId
join (select item.Server as DupServer, item.ItemName as DupName, MAX(item.EQitemId) DupItemId from EQitems item
group by item.Server, item.ItemName
having count(*)>1) as dups on eqitem.ItemName = dups.DupName AND eqitem.Server = dups.DupServer");
            _ = dbcontext.Database.ExecuteSqlRaw(@"delete from EQitems
where EQitemId IN (select MIN(item.EQitemId) EQitemId from EQitems item
group by item.Server, item.ItemName
having count(*)>1)");
            _ = dbcontext.Database.ExecuteSqlRaw(@"delete from EQitems
where EQitemId IN (select MIN(item.EQitemId) EQitemId from EQitems item
group by item.Server, item.ItemName
having count(*)>1)");
            _ = dbcontext.Database.ExecuteSqlRaw(@"delete from EQitems
where EQitemId IN (select MIN(item.EQitemId) EQitemId from EQitems item
group by item.Server, item.ItemName
having count(*)>1)");
        }

        public void FixOutlierDataMaxCleanup()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            _ = dbcontext.Database.ExecuteSqlRaw(@"update eqpi
set eqpi.AuctionPrice = null 
from EQTunnelAuctionItems eqpi
join EQitems eqitem on eqitem.EQitemId = eqpi.EQitemId
where eqpi.AuctionPrice is not null and (eqpi.AuctionPrice > eqitem.TotalWTSLast6MonthsAverage * 7 ) AND eqitem.TotalWTSLast6MonthsAverage > 40");
        }

        public void FixOutlierDataAfterMaxCleanup()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            _ = dbcontext.Database.ExecuteSqlRaw(@"update eqpi
set eqpi.AuctionPrice = null 
from EQTunnelAuctionItems eqpi
join EQitems eqitem on eqitem.EQitemId = eqpi.EQitemId
where eqpi.AuctionPrice is not null and eqpi.AuctionPrice < eqitem.TotalWTSLast6MonthsAverage * .1 AND eqitem.TotalWTSLast6MonthsAverage >40");
        }

        public void DeleteApiLogs()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            _ = dbcontext.Database.ExecuteSqlRaw(@"delete from [dbo].[APILogs] where CreatedDate < DATEADD(day, -30, SYSDATETIME())");
        }
    }
}
