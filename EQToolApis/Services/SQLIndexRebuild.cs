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

        public void RebuildEQAuctionPlayers()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(20));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQAuctionPlayers] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }
        public void RebuildEQitems()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(20));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EqToolExceptions] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQitems] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }
        public void RebuildEQTunnelAuctionItems()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(20));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQTunnelAuctionItems] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }
        public void RebuildEQTunnelAuctionEQTunnelMessages()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(20));
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [EQTunnelMessages] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
            _ = dbcontext.Database.ExecuteSqlRaw("ALTER INDEX ALL ON [Players] REBUILD WITH (FILLFACTOR = 80, SORT_IN_TEMPDB = ON, STATISTICS_NORECOMPUTE = ON)");
        }

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

        public void MessageDupFix()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            _ = dbcontext.Database.ExecuteSqlRaw(@"delete eqit 
from EQTunnelMessages eqit where DiscordMessageId IN (select top 2000 DiscordMessageId from EQTunnelMessages
group by DiscordMessageId
having count(*) >1)");
        }
        public void FixOutlierData()
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            _ = dbcontext.Database.ExecuteSqlRaw(@"update eqpi
set eqpi.AuctionPrice = null
from EQTunnelAuctionItems eqpi
join EQitems eqitem on eqitem.EQitemId = eqpi.EQitemId
where eqpi.AuctionPrice is not null and (eqpi.AuctionPrice < eqitem.TotalWTSLast90DaysAverage * .2 OR eqpi.AuctionPrice > eqitem.TotalWTSLast90DaysAverage * 3 ) AND eqitem.TotalWTSLast90DaysAverage >100");
        }
    }
}
