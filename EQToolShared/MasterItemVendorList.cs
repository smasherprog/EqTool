using System;
using System.Collections.Generic;

namespace EQToolShared
{
    public class MasterItemVendorListItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Buy { get; set; }
        public int Sell { get; set; }
    }
    public static class MasterItemVendorList
    {
        public static Dictionary<string, MasterItemVendorListItem> VendorItems = new Dictionary<string, MasterItemVendorListItem>(StringComparer.OrdinalIgnoreCase);
        static MasterItemVendorList()
        {
            var temp = Properties.Resources.items_vendor_prices.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in temp)
            {
                var parts = item.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    if (int.TryParse(parts[0], out var id) && int.TryParse(parts[2], out var buy) && int.TryParse(parts[3], out var sell))
                    {
                        VendorItems[parts[1].Trim()] = new MasterItemVendorListItem
                        {
                            ID = id,
                            Name = parts[1].Trim(),
                            Buy = buy,
                            Sell = sell
                        };
                    }
                }
            }
        }
    }
}
