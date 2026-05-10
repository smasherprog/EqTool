namespace EQToolApis.Models
{
    public class EQItemImage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;
    }

    public static class EQItemImageList
    {
        public static List<EQItemImage> Items { get; private set; } = [];

        public static void Load(string filePath)
        {
            var results = new List<EQItemImage>();
            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split('\t');
                if (parts.Length < 3)
                {
                    continue;
                }
                if (!int.TryParse(parts[0].Trim(), out var id))
                {
                    continue;
                }
                results.Add(new EQItemImage
                {
                    Id = id,
                    Name = parts[1].Trim(),
                    ImageName = parts[2].Trim()
                });
            }
            Items = results;
        }
    }
}
