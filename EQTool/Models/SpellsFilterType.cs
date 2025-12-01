using System.ComponentModel;
using EQTool.Services.TypeConverters;

namespace EQTool.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum SpellsFilterType
    {
        [Description("Allowed Classes")]
        ByClass = 0,
        [Description("Cast By You")]
        CastByYou = 1,
        [Description("Cast By Your Class")]
        CastByYourClass = 4,
    }
}
