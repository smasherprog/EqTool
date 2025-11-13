using System;

namespace EQToolShared.Extensions
{
    public static class StringExtensions
    {
        public static string TrimEnd(this string source, string value)
            => source.EndsWith(value) ? source.Remove(source.LastIndexOf(value, StringComparison.Ordinal)) : source;
        
        public static bool Contains(this string source,  string value, StringComparison comparisonType)
            => source.IndexOf(value, comparisonType) >= 0;
    }
}
