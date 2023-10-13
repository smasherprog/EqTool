using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            var pricestartindex = -1;
            for (var i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]) && pricestartindex == -1)
                {
                    pricestartindex = i;
                }

                if (!(input[i] == ' ' || char.IsLetterOrDigit(input[i]) || input[i] == '`' || input[i] == '.' || input[i] == '\''))
                {
                    itembreakindex = i;
                    break;
                }
            }

            if (itembreakindex == -1)
            {
                itembreakindex = input.Length;
                if (input.Length == 0 || string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }
            }
            if (pricestartindex == -1)
            {
                pricestartindex = itembreakindex;
            }

            var itemname = input.Substring(0, pricestartindex).Trim();
            if (string.IsNullOrWhiteSpace(itemname))
            {
                return null;
            }
            if (itemname.EndsWith(" x"))
            {
                itemname = itemname.Substring(0, itemname.Length - 2);
            }
            var price = (int?)null;
            if (pricestartindex != itembreakindex)
            {
                var pricestring = input.Substring(pricestartindex, itembreakindex - pricestartindex).Trim();
                if (!string.IsNullOrWhiteSpace(pricestring) && pricestring.IndexOf("x", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    var pricemultiple = 1;
                    if (pricestring.IndexOf("k", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        pricemultiple = 1000;
                    }
                    var startnumber = -1;
                    var endnumber = -1;
                    for (var i = 0; i < pricestring.Length; i++)
                    {
                        if (char.IsDigit(pricestring[i]) && startnumber == -1)
                        {
                            startnumber = i;
                        }
                        if (char.IsDigit(pricestring[i]) && startnumber != -1)
                        {
                            startnumber = i;
                        }
                    }

                    pricestring = new string(pricestring.Where(a => char.IsDigit(a)).ToArray());
                    if (int.TryParse(pricestring, out var possibleprice))
                    {
                        price = possibleprice * pricemultiple;
                    }
                }

            }
            itembreakindex = itembreakindex + 1 <= input.Length ? itembreakindex + 1 : itembreakindex;
            return new NextItem
            {
                Input = input.Substring(itembreakindex).Trim(),
                Name = itemname,
                Price = price
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
            //replace all instances of x15   or    x4 
            string pattern = @"x\d+";
            input = Regex.Replace(input, pattern, string.Empty);
            //replace all instances of (got 2)    or    (got a few) 
            pattern = @"\([^)]*\)";
            input = Regex.Replace(input, pattern, "/");

            var removetext = "/stack";
            var stackindex = input.IndexOf(removetext, StringComparison.OrdinalIgnoreCase);
            while (stackindex != -1)
            {
                input = input.Replace(input.Substring(stackindex, removetext.Length), string.Empty);
                stackindex = input.IndexOf(removetext, StringComparison.OrdinalIgnoreCase);
            }



            var auctiontype = GetAuctionType(null, input);
            if (!auctiontype.auctiontype.HasValue)
            {
                return null;
            }
            input = auctiontype.input.Trim('\'');
            NextItem item = null;
            var counter = 0;
            do
            {
                item = GetNextItem(input);
                auctiontype = GetAuctionType(auctiontype.auctiontype, input);
                input = auctiontype.input;
                if (item != null)
                {
                    input = item.Input;
                    ret.Items.Add(new Auctionitem
                    {
                        AuctionType = auctiontype.auctiontype.Value,
                        Name = item.Name,
                        Price = item.Price
                    });
                }
            } while (item != null && counter++ < 15);

            return ret;
        }
    }
}
