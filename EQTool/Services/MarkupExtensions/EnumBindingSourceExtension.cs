using System;
using System.Linq;
using System.Windows.Markup;

namespace EQTool.Services.MarkupExtensions
{
    // This extension will allow you to take an enum Type and then create a bindable list of enum values for a control.
    public class EnumBindingSourceExtension : MarkupExtension
    {
        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        /// <summary>
        /// Comma-separated list of enum names to exclude
        /// e.g. "BySpellExceptYou,SomethingElse"
        /// </summary>
        public string Exclude { get; set; }
        
        private Type _EnumType;
        public Type EnumType
        {
            get => _EnumType;
            set
            {
                if (value == _EnumType)
                    return;

                if (value != null)
                {
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;
                    if (!enumType.IsEnum)
                    {
                        throw new ArgumentException("Type must be for an Enum.");
                    }
                }

                _EnumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_EnumType == null)
            {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            var actualEnumType = Nullable.GetUnderlyingType(_EnumType) ?? _EnumType;
            var enumValues = Enum.GetValues(actualEnumType).Cast<object>();

            // Handle exclusions
            if (!string.IsNullOrWhiteSpace(Exclude))
            {
                var excludedNames = Exclude
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                enumValues = enumValues
                    .Where(v => !excludedNames.Contains(v.ToString()));
            }

            var final = enumValues.ToArray();

            // Nullable enum support
            if (actualEnumType == _EnumType)
                return final;

            var tempArray = Array.CreateInstance(actualEnumType, final.Length + 1);
            final.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
}
