using Microsoft.AspNetCore.Hosting;

namespace EQToolApis.Services
{
    public class ItemDataService
    {
        private readonly Dictionary<int, string> _imageById = new();
        private readonly Dictionary<string, string> _imageByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _tooltipByName = new(StringComparer.OrdinalIgnoreCase);

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
                        _tooltipByName.TryAdd(name, text.Trim());
                }
                pos = endIdx + endMarker.Length;
            }
        }

        // imglst.dat's first column is an internal pigparse row id, NOT the in-game
        // item id that inventory dumps contain (e.g. Backpack is 476 in imglst.dat
        // but 17969 in game), so matching by id shows the wrong item's icon. The
        // item name is the reliable key.
        public string GetImageUrl(int itemId, string itemName)
        {
            if (!string.IsNullOrEmpty(itemName) && _imageByName.TryGetValue(itemName.Trim(), out var img))
                return $"/Content/Images/{img}";
            return "/Content/Images/Item_.png";
        }

        public string GetTooltip(string itemName) =>
            _tooltipByName.TryGetValue(itemName, out var tt) ? tt : string.Empty;
    }
}
