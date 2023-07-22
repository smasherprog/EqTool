using System;
using System.Globalization;

namespace EQTool.Services.Parsing
{
    public static class LogFileDateTimeParse
    {
        public static DateTime ParseDateTime(string datestamp)
        {
            var format = "ddd MMM dd HH:mm:ss yyyy";
            try
            {
                return DateTime.ParseExact(datestamp, format, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
            }
            return DateTime.Now;
        }
    }
}
