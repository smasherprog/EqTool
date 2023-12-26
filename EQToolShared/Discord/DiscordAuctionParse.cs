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
        private bool IsAuctionTyoe(string input)
        {
            var searchstring = "wts";
            var searchstringindex = input.IndexOf(searchstring, StringComparison.OrdinalIgnoreCase);
            if (searchstringindex != -1)
            {
                return true;
            }
            searchstring = "wtb";
            searchstringindex = input.IndexOf(searchstring, StringComparison.OrdinalIgnoreCase);
            if (searchstringindex != -1)
            {
                return true;
            }
            return false;
        }

        public class NextItem
        {
            public string Input { get; set; }
            public string Name { get; set; }
            public int? Price { get; set; }
            public AuctionType AuctionType { get; set; } = Enums.AuctionType.WTS;
        }

        private bool isPricing(string input, int i)
        {
            return (input[i] == '.' || char.ToLower(input[i]) == 'k' || char.ToLower(input[i]) == 'p' || char.ToLower(input[i]) == ' ' || char.IsDigit(input[i]));
        }

        private NextItem GetItem(string input)
        {
            var itembreakindex = -1;
            var pricestartindex = -1;
            var itemstartindex = -1;
            var itemname = string.Empty;
            foreach (var item in MasterItemList.Items)
            {
                itembreakindex = input.IndexOf(item, StringComparison.OrdinalIgnoreCase);
                if (itembreakindex != -1)
                {
                    itemname = item;
                    itemstartindex = itembreakindex;
                    pricestartindex = itembreakindex + item.Length;
                    break;
                }
            }
            if (pricestartindex != -1)
            {
                var pricingstart = pricestartindex + 1;
                pricestartindex = -1;
                for (var i = pricingstart; i < input.Length; i++)
                {
                    var item = input[i];
                    if (char.IsDigit(input[i]) && pricestartindex == -1)
                    {
                        pricestartindex = i;
                    }
                    if (item == ' ' && pricestartindex != -1)
                    {
                        itembreakindex = i;
                        break;
                    }
                    else if (isPricing(input, i))
                    {
                        itembreakindex = i;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                itembreakindex = input.Length;
                return null;
            }

            if (pricestartindex == -1)
            {
                pricestartindex = itembreakindex;
            }
            var auctiontype = AuctionType.WTS;
            var srchstring = input.Substring(0, itemstartindex);
            var wtsindex = srchstring.LastIndexOf("wts", StringComparison.OrdinalIgnoreCase);
            var wtbindex = srchstring.LastIndexOf("wtb", StringComparison.OrdinalIgnoreCase);
            if (wtsindex != -1 && wtsindex > wtbindex)
            {
                auctiontype = AuctionType.WTS;
            }
            if (wtbindex != -1 && wtbindex > wtsindex)
            {
                auctiontype = AuctionType.WTB;
            }

            var price = (int?)null;
            if (pricestartindex != itembreakindex)
            {
                var toolongprice = "10000000";
                var pricestring = input.Substring(pricestartindex, itembreakindex - pricestartindex + 1).Trim();
                if (pricestring.Length < toolongprice.Length)
                {
                    if (!string.IsNullOrWhiteSpace(pricestring) && pricestring.IndexOf("x", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        var pricemultiple = 1.0;
                        var periodindex = pricestring.IndexOf('.');
                        if (pricestring.IndexOf("k", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            pricemultiple = 1000.0;
                        }
                        if (periodindex != -1 && periodindex != 0 && periodindex + 1 < pricestring.Length)
                        {
                            if (char.IsDigit(pricestring[periodindex - 1]) && char.IsDigit(pricestring[periodindex + 1]))
                            {
                                pricemultiple = 1000.0;
                            }
                        }

                        pricestring = new string(pricestring.Where(a => char.IsDigit(a) || a == '.').ToArray());
                        if (double.TryParse(pricestring, out var possibleprice))
                        {
                            price = (int)(possibleprice * pricemultiple);
                        }
                    }
                }
            }
            itembreakindex = itembreakindex + 1 <= input.Length ? itembreakindex + 1 : itembreakindex;
            return new NextItem
            {
                Input = input.Substring(0, itemstartindex) + input.Substring(itembreakindex),
                Name = itemname,
                Price = price > 0 ? price : null,
                AuctionType = auctiontype
            };
        }

        private string Trim(string input)
        {
            var begintrim = -1;
            for (var i = 0; i < input.Length; i++)
            {
                if (!char.IsLetter(input[i]))
                {
                    begintrim = i;
                }
                else
                {
                    break;
                }
            }
            if (begintrim == -1)
            {
                return input;
            }
            return input.Substring(begintrim);
        }

        public Auction Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length > 1000) return null;
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

            //replace all instances of 15x   or    4x
            pattern = @"\d+x";
            input = Regex.Replace(input, pattern, string.Empty);

            //replace all instances of (got 2)    or    (got a few) 
            pattern = @"\((?!Azia|Beza)[^\)]*\)";
            string result = Regex.Replace(input, pattern, string.Empty);

            //replace all instances of x 4     or   x 7
            pattern = @"x \d+";
            input = Regex.Replace(input, pattern, string.Empty);

            var removestrings = new List<string>() { "/stack", "/ea", "price", "paying" };
            foreach (var removetext in removestrings)
            {
                var stackindex = input.IndexOf(removetext, StringComparison.OrdinalIgnoreCase);
                while (stackindex != -1)
                {
                    input = input.Replace(input.Substring(stackindex, removetext.Length), string.Empty);
                    stackindex = input.IndexOf(removetext, StringComparison.OrdinalIgnoreCase);
                }
            }

            var tempstring = string.Empty;
            for (var i = 0; i < input.Length; i++)
            {
                if (MasterItemList.ValidChars.Contains(input[i]) || char.IsDigit(input[i]) || input[i] == ' ')
                {
                    tempstring += input[i];
                }
                else
                {
                    tempstring += ' ';
                }
            }

            input = tempstring.Replace(" - ", " ");

            var auctiontype = IsAuctionTyoe(input);
            if (!auctiontype)
            {
                return null;
            }
            input = input.Trim('\'');

            NextItem item = null;
            var counter = 0;
            do
            {
                item = GetItem(input);
                if (item != null)
                {
                    input = item.Input;
                    input = Trim(input);
                    if (MasterItemList.Items.Contains(item.Name))
                    {
                        ret.Items.Add(new Auctionitem
                        {
                            AuctionType = item.AuctionType,
                            Name = item.Name,
                            Price = item.Price
                        });
                    }
                }
            } while (item != null && input.Length > 0 && counter++ < 15);

            if (ret.Items.Any())
            {
                return ret;
            }

            return null;
        }
    }
}
