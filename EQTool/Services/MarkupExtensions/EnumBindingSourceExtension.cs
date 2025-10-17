using System;
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
        
        private Type _EnumType;
        public Type EnumType
        {
            get => _EnumType;
            set
            {
                if (value == _EnumType)
                {
                    return;
                }

                if (null != value)
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
            if (null == _EnumType)
            {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            var actualEnumType = Nullable.GetUnderlyingType(_EnumType) ?? _EnumType;
            var enumValues = Enum.GetValues(actualEnumType);
            if (actualEnumType == _EnumType)
            {
                return enumValues;
            }

            var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
}
