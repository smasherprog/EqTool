using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace EQToolApis.Services
{
    public class ItemDataService
    {
        private readonly Dictionary<int, string> _imageById = new();
        private readonly Dictionary<string, string> _imageByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _tooltipByName = new(StringComparer.OrdinalIgnoreCase);
        // Normalized fallbacks: same values keyed by NormalizeName() so a lookup
        // still hits when the inventory dump and the data files disagree on
        // apostrophe style (' vs `) or on whitespace.
        private readonly Dictionary<string, string> _imageByNorm = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _tooltipByNorm = new(StringComparer.OrdinalIgnoreCase);

        public ItemDataService(IWebHostEnvironment env)
        {
            LoadImages(env.WebRootPath);
            LoadTooltips(env.WebRootPath);
        }

        private void LoadImages(string webRoot)
        {
            var path = Path.Combine(webRoot, "Content", "imglst.dat");
            if (!File.Exists(path)) return;
            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split('\t');
                if (parts.Length < 3 || !int.TryParse(parts[0].Trim(), out var id)) continue;
                var name = parts[1].Trim();
                var img = parts[2].Trim();
                _imageById[id] = img;
                _imageByName.TryAdd(name, img);
                _imageByNorm.TryAdd(NormalizeName(name), img);
            }
        }

        private void LoadTooltips(string webRoot)
        {
            var path = Path.Combine(webRoot, "Content", "itemdata.dat");
            if (!File.Exists(path)) return;
            var content = File.ReadAllText(path);
            const string endMarker = "\t[end]";
            const string startMarker = "\t[start]\t\"";
            var pos = 0;
            while (pos < content.Length)
            {
                var endIdx = content.IndexOf(endMarker, pos, StringComparison.Ordinal);
                if (endIdx < 0) break;
                var chunk = content.Substring(pos, endIdx - pos);
                var startIdx = chunk.IndexOf(startMarker, StringComparison.Ordinal);
                if (startIdx >= 0)
                {
                    var name = chunk[..startIdx].Trim('\r', '\n', '\t', ' ');
                    var text = chunk[(startIdx + startMarker.Length)..].TrimEnd('"', '\r', '\n');
                    if (!string.IsNullOrEmpty(name))
                    {
                        _tooltipByName.TryAdd(name, text.Trim());
                        _tooltipByNorm.TryAdd(NormalizeName(name), text.Trim());
                    }
                }
                pos = endIdx + endMarker.Length;
            }
        }

        private const string ScrollIcon = "Item_869.png";
        private const string PlaceholderImage = "/Content/Images/Item_.png";

        // imglst.dat assigns some items an unrelated icon; these win over the file.
        private static readonly Dictionary<string, string> IconOverrides = new(StringComparer.OrdinalIgnoreCase)
        {
            // imglst.dat maps Water Flask to the maroon wine-bottle icon (704);
            // use the clear-bottle icon the other Flask items share.
            ["Water Flask"] = "Item_584.png",
        };

        // imglst.dat's first column is an internal pigparse row id, NOT the in-game
        // item id that inventory dumps contain (e.g. Backpack is 476 in imglst.dat
        // but 17969 in game), so matching by id shows the wrong item's icon. The
        // item name is the reliable key.
        public string GetImageUrl(int itemId, string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return PlaceholderImage;

            var trimmed = itemName.Trim();

            // Every "Spell: ..." item is a spell scroll in game, but imglst.dat
            // mis-icons most of them (chests, rods, armor), so force the scroll.
            if (trimmed.StartsWith("Spell: ", StringComparison.OrdinalIgnoreCase))
                return $"/Content/Images/{ScrollIcon}";

            if (IconOverrides.TryGetValue(trimmed, out var over))
                return $"/Content/Images/{over}";

            if (_imageByName.TryGetValue(trimmed, out var img))
                return $"/Content/Images/{img}";
            if (_imageByNorm.TryGetValue(NormalizeName(itemName), out img))
                return $"/Content/Images/{img}";

            return PlaceholderImage;
        }

        public string GetTooltip(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return string.Empty;
            if (_tooltipByName.TryGetValue(itemName.Trim(), out var tt))
                return tt;
            return _tooltipByNorm.TryGetValue(NormalizeName(itemName), out tt) ? tt : string.Empty;
        }

        // Unifies apostrophe variants to a backtick (EQ's convention) and collapses
        // internal whitespace so names match despite cosmetic differences between an
        // inventory dump and the bundled data files.
        private static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            var sb = new StringBuilder(name.Length);
            var lastWasSpace = false;
            foreach (var ch in name)
            {
                if (ch is '\'' or '`' or '‘' or '’' or '´')
                {
                    sb.Append('`');
                    lastWasSpace = false;
                }
                else if (char.IsWhiteSpace(ch))
                {
                    if (!lastWasSpace && sb.Length > 0)
                        sb.Append(' ');
                    lastWasSpace = true;
                }
                else
                {
                    sb.Append(ch);
                    lastWasSpace = false;
                }
            }
            return sb.ToString().TrimEnd();
        }
    }
}
