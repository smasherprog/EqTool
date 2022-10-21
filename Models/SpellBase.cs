namespace EQTool.Models
{
    /// <summary>
    /// 
    /// struct SPDat_Spell_Struct

    ///* 000 */	int			id;	// not used
    ///* 001 */	char		name[32]; // Name of the spell
    ///* 002 */	char		player_1[32]; // "PLAYER_1"
    ///* 003 */	char		teleport_zone[32];	// Teleport zone, pet name summoned, or item summoned
    ///* 004 */	char		you_cast[64]; // Message when you cast
    ///* 005 */	char		other_casts[64]; // Message when other casts
    ///* 006 */	char		cast_on_you[64]; // Message when spell is cast on you 
    ///* 007 */	char		cast_on_other[64]; // Message when spell is cast on someone else
    ///* 008 */	char		spell_fades[64]; // Spell fades
    ///* 009 */	float		range;
    ///* 010 */	float		aoerange;
    ///* 011 */	float		pushback;
    ///* 012 */	float		pushup;
    ///* 013 */	int32		cast_time; // Cast time
    ///* 014 */	int32		recovery_time; // Recovery time
    ///* 015 */	int32		recast_time; // Recast same spell time
    ///* 016 */	int32		buffdurationformula;
    ///* 017 */	int32		buffduration;
    ///* 018 */	int32		AEDuration;	// sentinel, rain of something
    ///* 019 */	int16		mana; // Mana Used
    ///* 020 */	sint16		base[EFFECT_COUNT];	//various purposes
    ///* 032 */	int			base2[12];			//various purposes
    ///* 044 */	sint16		max[EFFECT_COUNT];
    ///* 056 */	int16		icon; // Spell icon
    ///* 057 */	int16		memicon; // Icon on membarthing
    ///* 058 */	sint16		components[4]; // reagents
    ///* 062 */	int			component_counts[4]; // amount of regents used
    ///* 066 */	signed		NoexpendReagent[4];	// focus items (Need but not used; Flame Lick has a Fire Beetle Eye focus.)
    //											// If it is a number between 1-4 it means components[number] is a focus and not to expend it
    //											// If it is a valid itemid it means this item is a focus as well
    ///* 070 */	int16		formula[EFFECT_COUNT]; // Spell's value formula
    ///* 082 */	int			LightType; // probaly another effecttype flag
    ///* 083 */	int			goodEffect; //0=detrimental, 1=Beneficial, 2=Beneficial, Group Only
    ///* 084 */	int			Activated; // probaly another effecttype flag	
    ///* 085 */	int			resisttype;
    ///* 086 */	int			effectid[EFFECT_COUNT];	// Spell's effects
    ///* 098 */	SpellTargetType	targettype;	// Spell's Target
    ///* 099 */	int			basediff; // base difficulty fizzle adjustment
    ///* 100 */	int			skill;
    ///* 101 */	sint16		zonetype;	// 01=Outdoors, 02=dungeons, ff=Any 
    ///* 102 */	int16		EnvironmentType;
    ///* 103 */	int			TimeOfDay;
    ///* 104 */	int8		classes[PLAYER_CLASS_COUNT]; // Classes, and their min levels
    ///* 120 */	int8		CastingAnim;
    ///* 121 */	int8		TargetAnim;
    ///* 122 */	int32		TravelType;
    ///* 123 */	int16		SpellAffectIndex;
    ///* 124 */	int16		Spacing2[23];
    ///* 147 */	sint16		ResistDiff;
    ///* 148 */	int16		Spacing3[2];
    ///* 150 */	int16		RecourseLink;
    ///* 151 */	int			Spacing4[4];
    ///* 155 */	int			descnum; // eqstr of description of spell
    ///* 156 */	int			typedescnum; // eqstr of type description
    ///* 157 */	int			effectdescnum; // eqstr of effect description
    ///* 158 */	int			Spacing5[17];
    ///* 175 */	// last field is 174

    //Some fields which should be in here somewhere (MQ2):
    //Deletable
    //PvPResistBase
    //PvPResistCalc

    public class SpellBase
    {
        public int id { get; set; } // not used 
        public string name { get; set; } // Name of the spell  
        public string cast_on_you { get; set; }
        public string cast_on_other { get; set; }
        public int Level { get; set; }
        public int casttime { get; set; }
        public string spell_fades { get; set; }
        public int buffdurationformula { get; set; }
        public int pvp_buffdurationformula { get; set; }
        public int spell_icon { get; set; }
        public int buffduration { get; set; }
    }
}
