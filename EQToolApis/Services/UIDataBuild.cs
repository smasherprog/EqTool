using EQToolApis.DB;
using EQToolApis.Models;
using Microsoft.EntityFrameworkCore;

namespace EQToolApis.Services
{
    public class ServerData
    {
        public int TotalEQTunnelMessages;

        public int TotalEQTunnelAuctionItems;

        public DateTimeOffset RecentImportTimeStamp { get; set; }

        public DateTimeOffset OldestImportTimeStamp { get; set; }
    }

    public class AllData
    {
        public int TotalEQAuctionPlayers;

        public int TotalUniqueItems;

        public ServerData GreenServerData { get; set; } = new ServerData();

        public ServerData BlueServerData { get; set; } = new ServerData();
    }

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
                        LastSeen = a.LastWTSSeen.Value,
                        AuctionType = AuctionType.WTS
                    }).ToList();

            var wtb = dbcontext.EQitems
                .Where(a => a.Server == server && a.LastWTBSeen.HasValue)
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
                    LastSeen = a.LastWTBSeen.Value,
                    AuctionType = AuctionType.WTB
                }).ToList();

            ItemCache[(int)server] = wts.Concat(wtb)
            .OrderBy(a => a.ItemName)
            .ThenBy(a => a.AuctionType)
            .ToList();
        }
    }
}
