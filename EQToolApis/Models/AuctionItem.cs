using EQToolApis.DB;

namespace EQToolApis.Models
{
    public class AuctionItem
    {
        public int i { get; set; }

        public AuctionType t { get; set; }

        public string n { get; set; }

        public DateTimeOffset l { get; set; }

        public int tc { get; set; }

        public int ta { get; set; }

        public int t30 { get; set; }

        public int a30 { get; set; }

        public int t60 { get; set; }

        public int a60 { get; set; }

        public int t90 { get; set; }

        public int a90 { get; set; }

        public int t6m { get; set; }

        public int a6m { get; set; }

        public int ty { get; set; }

        public int ay { get; set; }
    }
}
