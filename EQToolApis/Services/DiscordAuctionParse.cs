using EQToolShared.Enums;

namespace EQToolApis.Services
{
    public class Auctionitem
    {
        public AuctionType AuctionType { get; set; }
        public string Name { get; set; }
        public DateTimeOffset TunnelTimestamp { get; set; }
        public int? Price { get; set; }
    }

    public static class DiscordAuctionParse
    {
        public static List<Auctionitem> Parse(string input)
        {
            var ret = new List<Auctionitem>();


            return ret;
        }
    }
}
