using EQToolApis.DB;
using EQToolApis.Models;
using Hangfire;

namespace EQToolApis.Services
{
    public class UIDataBuild
    {
        private readonly EQToolContext dbcontext;

        public static List<AuctionItem>[] ItemCache = new List<AuctionItem>[(int)(Servers.Blue + 1)];
        private readonly IBackgroundJobClient backgroundJobClient;

        public UIDataBuild(EQToolContext dbcontext, IBackgroundJobClient backgroundJobClient)
        {
            this.dbcontext = dbcontext;
            this.backgroundJobClient = backgroundJobClient;
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
            var items = new List<AuctionItem>();

            var wts = dbcontext.EQitems
                    .Where(a => a.Server == server)
                    .Select(a => new AuctionItem
                    {
                        ItemId = a.EQitemId,
                        ItemName = a.ItemName,
                        TotalLast30DaysAverage = a.TotalWTSLast30DaysAverage,
                        TotalLast30DaysCount = a.TotalWTSLast30DaysCount,
                        TotalLast60DaysCount = a.TotalWTSLast60DaysCount,
                        TotalLast60DaysAverage = a.TotalWTSLast60DaysAverage,
                        TotalLast90DaysCount = a.TotalWTSLast90DaysCount,
                        TotalLast90DaysAverage = a.TotalWTSLast90DaysAverage,
                        TotalLast6MonthsCount = a.TotalWTSLast6MonthsCount,
                        TotalLast6MonthsAverage = a.TotalWTSLast6MonthsAverage,
                        TotalAuctionCount = a.TotalWTSAuctionCount,
                        TotalAuctionAverage = a.TotalWTSAuctionAverage,
                        TotalLastYearAverage = a.TotalWTSLastYearAverage,
                        TotalLastYearCount = a.TotalWTSLastYearCount,
                        LastSeen = a.LastWTSSeen,
                        AuctionType = AuctionType.WTS
                    }).ToList();

            var wtb = dbcontext.EQitems
                .Where(a => a.Server == server)
                .Select(a => new AuctionItem
                {
                    ItemId = a.EQitemId,
                    ItemName = a.ItemName,
                    TotalLast30DaysAverage = a.TotalWTBLast30DaysAverage,
                    TotalLast30DaysCount = a.TotalWTBLast30DaysCount,
                    TotalLast60DaysCount = a.TotalWTBLast60DaysCount,
                    TotalLast60DaysAverage = a.TotalWTBLast60DaysAverage,
                    TotalLast90DaysCount = a.TotalWTBLast90DaysCount,
                    TotalLast90DaysAverage = a.TotalWTBLast90DaysAverage,
                    TotalLast6MonthsCount = a.TotalWTBLast6MonthsCount,
                    TotalLast6MonthsAverage = a.TotalWTBLast6MonthsAverage,
                    TotalAuctionCount = a.TotalWTBAuctionCount,
                    TotalAuctionAverage = a.TotalWTBAuctionAverage,
                    TotalLastYearAverage = a.TotalWTBLastYearAverage,
                    TotalLastYearCount = a.TotalWTBLastYearCount,
                    LastSeen = a.LastWTBSeen,
                    AuctionType = AuctionType.WTB
                }).ToList();

            ItemCache[(int)server] = wts.Concat(wtb)
            .OrderBy(a => a.ItemName)
            .ThenBy(a => a.AuctionType)
            .ToList();
        }
    }
}
