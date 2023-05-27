using System;

namespace EQTool
{
    public class CustomTimeSpanUpDown : Xceed.Wpf.Toolkit.TimeSpanUpDown
    {
        protected override TimeSpan? ConvertTextToValue(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            //dd,hh:mm
            return new TimeSpan(0, Convert.ToInt32(text.Substring(0, 2)), Convert.ToInt32(text.Substring(3, 2)), 0);
        }

        protected override string ConvertValueToText()
        {
            return !Value.HasValue ? string.Empty : Value.Value.ToString(@"hh\:mm");
        }

        protected override void OnIncrement()
        {
            if (Value.HasValue)
            {
                Value = Value.Value.Add(TimeSpan.FromMinutes(1));
            }
        }

        protected override void OnDecrement()
        {
            if (Value.HasValue)
            {
                Value = Value.Value.Add(TimeSpan.FromMinutes(-1));
            }
        }
    }
}
