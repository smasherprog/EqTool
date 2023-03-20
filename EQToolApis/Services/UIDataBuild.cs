using EQToolApis.DB;

namespace EQToolApis.Services
{
    public class UIDataBuild
    {
        private readonly EQToolContext dbcontext;

        public static List<AuctionItem> ItemCache = new();

        public UIDataBuild(EQToolContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public void BuildData()
        {
            var wts = dbcontext.EQTunnelAuctionItems
                       .Where(a => a.EQTunnelMessage.AuctionType == AuctionType.WTS)
                       .GroupBy(a => new AuctionItem
                       {
                           AuctionType = a.EQTunnelMessage.AuctionType,
                           ItemName = a.EQitem.ItemName,
                           TotalLast30DaysAverage = a.EQitem.TotalWTSLast30DaysAverage,
                           TotalLast30DaysCount = a.EQitem.TotalWTSLast30DaysCount,
                           TotalLast60DaysCount = a.EQitem.TotalWTSLast60DaysCount,
                           TotalLast60DaysAverage = a.EQitem.TotalWTSLast60DaysAverage,
                           TotalLast90DaysCount = a.EQitem.TotalWTSLast90DaysCount,
                           TotalLast90DaysAverage = a.EQitem.TotalWTSLast90DaysAverage,
                           TotalLast6MonthsCount = a.EQitem.TotalWTSLast6MonthsCount,
                           TotalLast6MonthsAverage = a.EQitem.TotalWTSLast6MonthsAverage,
                           TotalAuctionCount = a.EQitem.TotalWTSAuctionCount,
                           TotalAuctionAverage = a.EQitem.TotalWTSAuctionAverage,
                           LastSeen = a.EQitem.LastWTSSeen
                       })
                       .Select(a => a.Key).ToList();

            var wtb = dbcontext.EQTunnelAuctionItems
              .Where(a => a.EQTunnelMessage.AuctionType == AuctionType.WTB)
              .GroupBy(a => new AuctionItem
              {
                  AuctionType = a.EQTunnelMessage.AuctionType,
                  ItemName = a.EQitem.ItemName,
                  TotalLast30DaysAverage = a.EQitem.TotalWTBLast30DaysAverage,
                  TotalLast30DaysCount = a.EQitem.TotalWTBLast30DaysCount,
                  TotalLast60DaysCount = a.EQitem.TotalWTBLast60DaysCount,
                  TotalLast60DaysAverage = a.EQitem.TotalWTBLast60DaysAverage,
                  TotalLast90DaysCount = a.EQitem.TotalWTBLast90DaysCount,
                  TotalLast90DaysAverage = a.EQitem.TotalWTBLast90DaysAverage,
                  TotalLast6MonthsCount = a.EQitem.TotalWTBLast6MonthsCount,
                  TotalLast6MonthsAverage = a.EQitem.TotalWTBLast6MonthsAverage,
                  TotalAuctionCount = a.EQitem.TotalWTBAuctionCount,
                  TotalAuctionAverage = a.EQitem.TotalWTBAuctionAverage,
                  LastSeen = a.EQitem.LastWTBSeen
              })
              .Select(a => a.Key).ToList();

            ItemCache = wts.Concat(wtb)
            .OrderBy(a => a.ItemName)
            .ThenBy(a => a.AuctionType)
            .ToList();
        }
    }
}
