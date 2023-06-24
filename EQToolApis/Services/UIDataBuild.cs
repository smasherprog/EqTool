using EQToolApis.DB;
using EQToolApis.Models;
using EQToolShared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.Services
{

    public class UIDataBuild
    {
        private readonly EQToolContext dbcontext;

        public static List<AuctionItem>[] ItemCache = new List<AuctionItem>[(int)(Servers.Blue + 1)];

        public UIDataBuild(EQToolContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public void BuildDataGreen()
        {
            BuildData(Servers.Green);
        }

        public void BuildDataBlue()
        {
            BuildData(Servers.Blue);
        }

        public void BuildData(Servers server)
        {
            dbcontext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            var items = new List<AuctionItem>();

            var wts = dbcontext.EQitems
                    .Where(a => a.Server == server && a.LastWTSSeen.HasValue)
                    .Select(a => new AuctionItem
                    {
                        i = a.EQitemId,
                        n = a.ItemName,
                        a30 = a.TotalWTSLast30DaysAverage,
                        t30 = a.TotalWTSLast30DaysCount,
                        t60 = a.TotalWTSLast60DaysCount,
                        a60 = a.TotalWTSLast60DaysAverage,
                        t90 = a.TotalWTSLast90DaysCount,
                        a90 = a.TotalWTSLast90DaysAverage,
                        t6m = a.TotalWTSLast6MonthsCount,
                        a6m = a.TotalWTSLast6MonthsAverage,
                        tc = a.TotalWTSAuctionCount,
                        ta = a.TotalWTSAuctionAverage,
                        ay = a.TotalWTSLastYearAverage,
                        ty = a.TotalWTSLastYearCount,
                        l = a.LastWTSSeen.Value,
                        t = AuctionType.WTS
                    }).ToList();

            var wtb = dbcontext.EQitems
                .Where(a => a.Server == server && a.LastWTBSeen.HasValue)
                .Select(a => new AuctionItem
                {
                    i = a.EQitemId,
                    n = a.ItemName,
                    a30 = a.TotalWTBLast30DaysAverage,
                    t30 = a.TotalWTBLast30DaysCount,
                    t60 = a.TotalWTBLast60DaysCount,
                    a60 = a.TotalWTBLast60DaysAverage,
                    t90 = a.TotalWTBLast90DaysCount,
                    a90 = a.TotalWTBLast90DaysAverage,
                    t6m = a.TotalWTBLast6MonthsCount,
                    a6m = a.TotalWTBLast6MonthsAverage,
                    tc = a.TotalWTBAuctionCount,
                    ta = a.TotalWTBAuctionAverage,
                    ay = a.TotalWTBLastYearAverage,
                    ty = a.TotalWTBLastYearCount,
                    l = a.LastWTBSeen.Value,
                    t = AuctionType.WTB
                }).ToList();

            ItemCache[(int)server] = wts.Concat(wtb)
            .OrderBy(a => a.n)
            .ThenBy(a => a.t)
            .ToList();
        }
    }
}
