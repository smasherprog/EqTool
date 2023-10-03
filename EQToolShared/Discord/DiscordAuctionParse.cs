using EQToolShared.Enums;
using System;
using System.Collections.Generic;

namespace EQToolShared.Discord
{
    public class Auctionitem
    {
        public AuctionType AuctionType { get; set; }
        public string Name { get; set; }
        public int? Price { get; set; }
    }

    public class Auction
    {
        public string Player { get; set; }
        public DateTimeOffset TunnelTimestamp { get; set; }
        public List<Auctionitem> Items { get; set; } = new List<Auctionitem>();
    }

    public class DiscordAuctionParse
    {
        private (AuctionType? auctiontype, string input) GetAuctionType(AuctionType? currenttype, string input)
        {
            var searchstring = "wts";
            var searchstringindex = input.IndexOf(searchstring, StringComparison.OrdinalIgnoreCase);
            if (searchstringindex != 0)
            {
                searchstring = "wtb";
                searchstringindex = input.IndexOf(searchstring, StringComparison.OrdinalIgnoreCase);
                if (searchstringindex != 0)
                {
                    return (currenttype, input);
                }
                return (AuctionType.WTB, input.Substring(searchstringindex + searchstring.Length).Trim());
            }
            return (AuctionType.WTS, input.Substring(searchstringindex + searchstring.Length).Trim());
        }

        public class NextItem
        {
            public string Input { get; set; }
            public string Name { get; set; }
            public int? Price { get; set; }
        }

        private NextItem GetNextItem(string input)
        {
            var itembreakindex = -1;
            for (var i = 0; i < input.Length; i++)
            {
                if (!(input[i] == ' ' || char.IsLetter(input[i]) || input[i] == '`' || input[i] == '\''))
                {
                    itembreakindex = i;
                    break;
                }
            }

            if (itembreakindex == -1)
            {
                return null;
            }
            return new NextItem
            {
                Input = input.Substring(itembreakindex).Trim(),
                Name = input.Substring(0, itembreakindex).Trim()
            };
        }


        public Auction Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var ret = new Auction();
            var searchstring = " auctions, '";
            var searchstringindex = input.IndexOf(searchstring, StringComparison.OrdinalIgnoreCase);
            if (searchstringindex == -1)
            {
                return null;
            }
            ret.Player = input.Substring(0, searchstringindex).Trim();
            input = input.Substring(searchstringindex + searchstring.Length);
            var auctiontype = GetAuctionType(null, input);
            if (!auctiontype.auctiontype.HasValue)
            {
                return null;
            }
            input = auctiontype.input;
            NextItem item = null;
            var counter = 0;
            do
            {
                item = GetNextItem(input);
                if (item != null)
                {
                    input = item.Input;
                    ret.Items.Add(new Auctionitem
                    {
                        AuctionType = auctiontype.auctiontype.Value,
                        Name = item.Name,
                        Price = null
                    });
                }
            } while (item != null && counter++ < 15);

            return ret;
        }
    }
}
