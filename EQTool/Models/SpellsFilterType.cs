using System.ComponentModel;
using EQTool.TypeConverters;

namespace EQTool.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum SpellsFilterType
    {
        [Description("By Class")]
        ByClass,
        [Description("Cast On You")]
        CastOnYou,
        [Description("Cast By You")]
        CastByYou,
        [Description("Cast By or On You")]
        CastByOrOnYou
    }
}
