using Newtonsoft.Json;
using System.IO;

namespace EQTool.Services
{
    // Reads and writes the spell cache. It was a BinaryFormatter file until .NET removed
    // BinaryFormatter, so bump the cache's name whenever the shape of what it stores changes.
    public static class CacheSerializer
    {
        public static void WriteToFile<T>(string filePath, T objectToWrite)
        {
            using (var writer = new StreamWriter(filePath))
            {
                JsonSerializer.Create().Serialize(writer, objectToWrite);
            }
        }

        public static T ReadFromFile<T>(string filePath)
        {
            using (var reader = new JsonTextReader(new StreamReader(filePath)))
            {
                return JsonSerializer.Create().Deserialize<T>(reader);
            }
        }
    }
}
