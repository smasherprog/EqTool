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
        private const string SpellText = "Spell:";

        private bool isSpell(string input, int i)
        {
            return i >= SpellText.Length - 1 && input[i] == ':' && input[i - 1] == 'l' && input[i - 2] == 'l' && input[i - 3] == 'e' && input[i - 4] == 'p' && input[i - 5] == 'S';
        }
        private bool isBeginPricing(string input, int i, int pricestartindex)
        {
            if (char.IsDigit(input[i]))
            {
                var index = input.IndexOf("Pg. ");
                if (index != -1)
                {
                    var diffindex = i - index;
                    if (diffindex <= 6)
                    {
                        return false;
                    }
                }
            }

            return char.IsDigit(input[i]) && pricestartindex == -1;
        }
        private bool isPricing(string input, int i, int pricestartindex)
        {
            return pricestartindex != -1 && (input[i] == '.' || char.ToLower(input[i]) == 'k' || char.ToLower(input[i]) == 'p');
        }
        private bool isEndPricing(string input, int i, int pricestartindex)
        {
            return pricestartindex != -1 && input[i] == ' ';
        }
        private bool isItemName(string input, int i)
        {
            return (input[i] == ' ' || char.IsLetterOrDigit(input[i]) || input[i] == '`' || input[i] == '.' || input[i] == '\'');
        }
        private NextItem GetNextItem(string input)
        {
            var itembreakindex = -1;
            var pricestartindex = -1;
            for (var i = 0; i < input.Length; i++)
            {
                if (isBeginPricing(input, i, pricestartindex))
                {
                    pricestartindex = i;
                }
                if (isPricing(input, i, pricestartindex))
                {
                    continue;
                }
                else if (isEndPricing(input, i, pricestartindex))
                {
                    itembreakindex = i;
                    break;
                }
                else if (isSpell(input, i))
                {
                    continue;
                }
                else if (!isItemName(input, i))
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
                    var pricemultiple = 1.0;
                    if (pricestring.IndexOf("k", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        pricemultiple = 1000.0;
                    }

                    pricestring = new string(pricestring.Where(a => char.IsDigit(a) || a == '.').ToArray());
                    if (double.TryParse(pricestring, out var possibleprice))
                    {
                        price = (int)(possibleprice * pricemultiple);
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

            //replace all instances of 15x   or    4x
            pattern = @"\d+x";
            input = Regex.Replace(input, pattern, string.Empty);

            //replace all instances of (got 2)    or    (got a few) 
            pattern = @"\([^)]*\)";
            input = Regex.Replace(input, pattern, "/");

            //replace all instances of x 4     or   x 7
            pattern = @"x \d+";
            input = Regex.Replace(input, pattern, string.Empty);


            var removetext = "/stack";
            var stackindex = input.IndexOf(removetext, StringComparison.OrdinalIgnoreCase);
            while (stackindex != -1)
            {
                input = input.Replace(input.Substring(stackindex, removetext.Length), string.Empty);
                stackindex = input.IndexOf(removetext, StringComparison.OrdinalIgnoreCase);
            }

            removetext = "/ea";
            stackindex = input.IndexOf(removetext, StringComparison.OrdinalIgnoreCase);
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
            input = Trim(input);
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
                    input = Trim(input);
                    if (MasterItemList.PQItems.Contains(item.Name) || MasterItemList.P99Items.Contains(item.Name))
                    {
                        ret.Items.Add(new Auctionitem
                        {
                            AuctionType = auctiontype.auctiontype.Value,
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
