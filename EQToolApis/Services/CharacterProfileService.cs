using EQToolApis.DB.Models;
using EQToolShared.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EQToolApis.Services
{
    public class ProfileItem
    {
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Capacity { get; set; }
        public int ItemId { get; set; }
    }

    // One general/bank slot: the item sitting in the slot (possibly a container)
    // plus the contents of that container when it has any capacity.
    public class ProfileBag
    {
        public ProfileItem? Container { get; set; }
        public List<ProfileItem?> Contents { get; set; } = [];
    }

    public class ProfileStats
    {
        public int AC { get; set; }
        public int HP { get; set; }
        public int Mana { get; set; }
        public int Atk { get; set; }
        public int Str { get; set; }
        public int Sta { get; set; }
        public int Agi { get; set; }
        public int Dex { get; set; }
        public int Wis { get; set; }
        public int Int { get; set; }
        public int Cha { get; set; }
        public int SvPoison { get; set; }
        public int SvMagic { get; set; }
        public int SvDisease { get; set; }
        public int SvFire { get; set; }
        public int SvCold { get; set; }
        public int Haste { get; set; }
        public double Weight { get; set; }
    }

    public class CharacterProfile
    {
        public string CharacterName { get; set; } = string.Empty;
        public Servers Server { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Dictionary<string, ProfileItem?> Equipped { get; set; } = [];
        public List<ProfileBag> General { get; set; } = [];
        public List<ProfileBag> Bank { get; set; } = [];
        public ProfileStats Stats { get; set; } = new();
    }

    public class CharacterProfileService
    {
        private readonly ItemDataService _itemData;

        public CharacterProfileService(ItemDataService itemData)
        {
            _itemData = itemData;
        }

        private static readonly Regex AttrRegex = new(@"\b(STR|STA|AGI|DEX|WIS|INT|CHA|HP|MANA|ATK):\s*([+-]\d+)", RegexOptions.Compiled);
        private static readonly Regex AcRegex = new(@"\bAC:\s*(-?\d+)", RegexOptions.Compiled);
        private static readonly Regex SvRegex = new(@"\bSV (POISON|MAGIC|DISEASE|FIRE|COLD):\s*([+-]\d+)", RegexOptions.Compiled);
        private static readonly Regex HasteRegex = new(@"\bHaste:\s*\+?(\d+)%", RegexOptions.Compiled);
        private static readonly Regex WtRegex = new(@"\bWT:\s*(\d+(?:\.\d+)?)", RegexOptions.Compiled);

        public CharacterProfile BuildProfile(CharacterInventory inventory)
        {
            var grouped = inventory.Items
                .GroupBy(i => i.Location)
                .ToDictionary(g => g.Key, g => g.ToList());

            var profile = new CharacterProfile
            {
                CharacterName = inventory.CharacterName,
                Server = inventory.Server,
                UpdatedAt = inventory.UpdatedAt,
            };

            ProfileItem? Slot(InventoryLocation loc, int idx = 0)
            {
                if (!grouped.TryGetValue(loc, out var items) || idx >= items.Count)
                {
                    return null;
                }

                var item = items[idx];
                // EQ's /outputfile inventory lists unused slots with the literal
                // name "Empty"; treat those as unoccupied so they render as blank
                // slots instead of a broken-icon placeholder.
                if (string.Equals(item.Name, "Empty", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return new ProfileItem
                {
                    Name = item.Name,
                    Image = _itemData.GetImageUrl(item.ItemId, item.Name),
                    Tooltip = _itemData.GetTooltip(item.Name),
                    Count = item.Count,
                    Capacity = item.Slots,
                    ItemId = item.ItemId,
                };
            }

            profile.Equipped = new Dictionary<string, ProfileItem?>
            {
                ["Charm"] = Slot(InventoryLocation.Charm),
                ["Ear1"] = Slot(InventoryLocation.Ear, 0),
                ["Head"] = Slot(InventoryLocation.Head),
                ["Face"] = Slot(InventoryLocation.Face),
                ["Ear2"] = Slot(InventoryLocation.Ear, 1),
                ["Neck"] = Slot(InventoryLocation.Neck),
                ["Shoulders"] = Slot(InventoryLocation.Shoulders),
                ["Arms"] = Slot(InventoryLocation.Arms),
                ["Back"] = Slot(InventoryLocation.Back),
                ["Wrist1"] = Slot(InventoryLocation.Wrist, 0),
                ["Wrist2"] = Slot(InventoryLocation.Wrist, 1),
                ["Range"] = Slot(InventoryLocation.Range),
                ["Hands"] = Slot(InventoryLocation.Hands),
                ["Primary"] = Slot(InventoryLocation.Primary),
                ["Secondary"] = Slot(InventoryLocation.Secondary),
                ["Finger1"] = Slot(InventoryLocation.Fingers, 0),
                ["Finger2"] = Slot(InventoryLocation.Fingers, 1),
                ["Chest"] = Slot(InventoryLocation.Chest),
                ["Legs"] = Slot(InventoryLocation.Legs),
                ["Feet"] = Slot(InventoryLocation.Feet),
                ["Waist"] = Slot(InventoryLocation.Waist),
                ["Ammo"] = Slot(InventoryLocation.Ammo),
                ["Held"] = Slot(InventoryLocation.Held),
            };

            ProfileBag Bag(string slotPrefix, InventoryLocation containerLoc)
            {
                var bag = new ProfileBag { Container = Slot(containerLoc) };
                var capacity = bag.Container?.Capacity ?? 0;
                for (var j = 1; j <= capacity; j++)
                {
                    bag.Contents.Add(Enum.TryParse<InventoryLocation>($"{slotPrefix}Slot{j}", out var loc) ? Slot(loc) : null);
                }
                return bag;
            }

            for (var i = 1; i <= 8; i++)
            {
                profile.General.Add(Bag($"General{i}", Enum.Parse<InventoryLocation>($"General{i}")));
            }

            // Classic-era servers (P99 through Velious, Quarm through PoP) only
            // expose 8 bank bag slots; higher slots never exist in-game, so they
            // would only render as permanently empty cells.
            for (var i = 1; i <= 8; i++)
            {
                profile.Bank.Add(Bag($"Bank{i}", Enum.Parse<InventoryLocation>($"Bank{i}")));
            }

            profile.Stats = BuildStats(profile);
            return profile;
        }

        // Stat totals come from parsing equipped item tooltips; weight covers
        // everything carried (equipped + general inventory and bag contents).
        private static ProfileStats BuildStats(CharacterProfile profile)
        {
            var stats = new ProfileStats();

            foreach (var item in profile.Equipped.Values.Where(i => i != null))
            {
                var tt = item!.Tooltip;
                if (string.IsNullOrEmpty(tt))
                {
                    continue;
                }

                foreach (Match m in AttrRegex.Matches(tt))
                {
                    var val = int.Parse(m.Groups[2].Value);
                    switch (m.Groups[1].Value)
                    {
                        case "STR": stats.Str += val; break;
                        case "STA": stats.Sta += val; break;
                        case "AGI": stats.Agi += val; break;
                        case "DEX": stats.Dex += val; break;
                        case "WIS": stats.Wis += val; break;
                        case "INT": stats.Int += val; break;
                        case "CHA": stats.Cha += val; break;
                        case "HP": stats.HP += val; break;
                        case "MANA": stats.Mana += val; break;
                        case "ATK": stats.Atk += val; break;
                    }
                }

                var ac = AcRegex.Match(tt);
                if (ac.Success)
                {
                    stats.AC += int.Parse(ac.Groups[1].Value);
                }

                foreach (Match m in SvRegex.Matches(tt))
                {
                    var val = int.Parse(m.Groups[2].Value);
                    switch (m.Groups[1].Value)
                    {
                        case "POISON": stats.SvPoison += val; break;
                        case "MAGIC": stats.SvMagic += val; break;
                        case "DISEASE": stats.SvDisease += val; break;
                        case "FIRE": stats.SvFire += val; break;
                        case "COLD": stats.SvCold += val; break;
                    }
                }

                // Worn haste does not stack in-game; the largest wins.
                var haste = HasteRegex.Match(tt);
                if (haste.Success)
                {
                    stats.Haste = Math.Max(stats.Haste, int.Parse(haste.Groups[1].Value));
                }
            }

            var carried = profile.Equipped.Values
                .Concat(profile.General.SelectMany(b => b.Contents.Prepend(b.Container)));
            foreach (var item in carried.Where(i => i != null))
            {
                var wt = WtRegex.Match(item!.Tooltip ?? string.Empty);
                if (wt.Success)
                {
                    stats.Weight += double.Parse(wt.Groups[1].Value, CultureInfo.InvariantCulture) * Math.Max(1, item.Count);
                }
            }
            stats.Weight = Math.Round(stats.Weight, 1);

            return stats;
        }
    }
}
