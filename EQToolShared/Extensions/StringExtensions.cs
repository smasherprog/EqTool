using System;

namespace EQToolShared.Extensions
{
    public static class StringExtensions
    {
        public static string TrimEnd(this string source, string value)
            => source.EndsWith(value) ? source.Remove(source.LastIndexOf(value, StringComparison.Ordinal)) : source;
    }
}
