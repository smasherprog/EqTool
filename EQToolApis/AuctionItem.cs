namespace EQToolApis
{
    public class AuctionItem
    {
        public DateTimeOffset LastSeen { get; set; }

        public string ItemName { get; set; }

        public int Count { get; set; }

        public int AveragePrice { get; set; }
    }
}
