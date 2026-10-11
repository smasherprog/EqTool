using System;
using System.Globalization;

namespace EQTool.Services.Parsing
{
    public static class LogFileDateTimeParse
    {
        private const string Format = "ddd MMM dd HH:mm:ss yyyy";

        public static DateTime ParseDateTime(string datestamp)
        {
            try
            {
                return DateTime.ParseExact(datestamp, Format, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
            }
            return DateTime.Now;
        }

        public static bool TryParseDateTime(string datestamp, out DateTime result)
        {
            return DateTime.TryParseExact(datestamp, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }
    }
}
