using System.ComponentModel;
using EQTool.Services.TypeConverters;

namespace EQTool.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum SpellGroupingType
    {
        [Description("By Target")]
        ByTarget,
        [Description("By Spell")]
        BySpell,
        [Description("By Spell (Except You)")]
        BySpellExceptYou
    }
}
