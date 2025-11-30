using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static void SafelyAdd<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue item)
    {
        if (dict.ContainsKey(key))
            dict[key].Add(item);
        else
            dict.Add(key, new List<TValue> {item});
    }
    
    public static void SafelyRemove<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue item)
    {
        if (!dict.TryGetValue(key, out var values))
            return;

        values.Remove(item);
        
        if (values.Count == 0)
            dict.Remove(key);
    }
}
