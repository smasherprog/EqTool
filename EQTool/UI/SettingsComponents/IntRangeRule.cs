using System.Globalization;
using System.Windows.Controls;

namespace EQTool.UI.SettingsComponents
{
    // Validation rule for the timer/counter number boxes: the text must be a whole
    // number within [Min, Max]. Runs against the raw typed text so non-numeric or
    // out-of-range input surfaces an error (and never reaches the bound int).
    public class IntRangeRule : ValidationRule
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = int.MaxValue;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return new ValidationResult(false, "Required");
            }
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                return new ValidationResult(false, "Must be a whole number");
            }
            if (number < Min || number > Max)
            {
                return new ValidationResult(false, $"Must be between {Min} and {Max}");
            }
            return ValidationResult.ValidResult;
        }
    }
}
