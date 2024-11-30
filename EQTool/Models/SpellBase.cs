using EQToolShared.Enums;
using System;
using System.Collections.Generic;

namespace EQTool.Models
{
    /// <summary>
    /// 
    /// struct SPDat_Spell_Struct

    ///* 000 */	int			id;	// not used
    ///* 001 */	char		name[32]; // Victim of the spell
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
    ///* 082 */	int			LightType; // probably another effecttype flag
    ///* 083 */	int			goodEffect; //0=detrimental, 1=Beneficial, 2=Beneficial, Group Only
    ///* 084 */	int			Activated; // probably another effecttype flag	
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

    public enum ResistType
    {
        NONE = 0,
        MAGIC = 1,
        FIRE = 2,
        COLD = 3,
        POISON = 4,
        DISEASE = 5,
        CHROMATIC = 6,
        PRISMATIC = 7,
        PHYSICAL = 8,
        CORRUPTION = 9
    }

    public enum SpellType
    {
        RagZhezumSpecial = 0,
        LineofSight = 1,
        GroupV1 = 3,
        PointBlankAreaofEffect = 4,
        Single = 5,
        Self = 6,
        TargetedAreaofEffect = 8,
        Animal = 9,
        Undead = 10,
        Summoned = 11,
        LifeTap = 13,
        Pet = 14,
        Corpse = 15,
        Plant = 16,
        UberGiants = 17,
        UberDragons = 18,
        TargetedAreaofEffectLifeTap = 20,
        AreaofEffectUndead = 24,
        AreaofEffectSummoned = 25,
        AreaofEffectCaster = 32,
        NPCHateList = 33,
        DungeonObject = 34,
        Muramite = 35,
        AreaPCOnly = 36,
        AreaNPCOnly = 37,
        SummonedPet = 38,
        GroupNoPets = 39,
        AreaofEffectPCV2 = 40,
        Groupv2 = 41,
        SelfDirectional = 42,
        GroupWithPets = 43,
        Beam = 44
    }

    public enum DescrNumber
    {
        Aegolism = 1,
        Agility = 2,
        Alliance = 3,
        Animal = 4,
        Antonica = 5,
        ArmorClass = 6,
        Attack = 7,
        Bane = 8,
        Blind = 9,
        Block = 10,
        Calm = 11,
        Charisma = 12,
        Charm = 13,
        Cold = 14,
        CombatAbilities = 15,
        CombatInnates = 16,
        Conversions = 17,
        CreateItem = 18,
        Cure = 19,
        DamageOverTime = 20,
        DamageShield = 21,
        Defensive = 22,
        Destroy = 23,
        Dexterity = 24,
        DirectDamage = 25,
        DisarmTraps = 26,
        Disciplines = 27,
        Discord = 28,
        Disease = 29,
        Disempowering = 30,
        Dispel = 31,
        DurationHeals = 32,
        DurationTap = 33,
        EnchantMetal = 34,
        Enthrall = 35,
        Faydwer = 36,
        Fear = 37,
        Fire = 38,
        FizzleRate = 39,
        Fumble = 40,
        Haste = 41,
        Heals = 42,
        Health = 43,
        HealthMana = 44,
        HPBuffs = 45,
        HPtypeone = 46,
        HPtypetwo = 47,
        IllusionOther = 48,
        IllusionPlayer = 49,
        ImbueGem = 50,
        Invisibility = 51,
        Invulnerability = 52,
        Jolt = 53,
        Kunark = 54,
        Levitate = 55,
        LifeFlow = 56,
        Luclin = 57,
        Magic = 58,
        Mana = 59,
        ManaDrain = 60,
        ManaFlow = 61,
        MeleeGuard = 62,
        MemoryBlur = 63,
        Misc = 64,
        Movement = 65,
        Objects = 66,
        Odus = 67,
        Offensive = 68,
        Pet = 69,
        PetHaste = 70,
        PetMiscBuffs = 71,
        Physical = 72,
        Picklock = 73,
        Plant = 74,
        Poison = 75,
        PowerTap = 76,
        QuickHeal = 77,
        Reflection = 78,
        Regen = 79,
        ResistBuff = 80,
        ResistDebuffs = 81,
        Resurrection = 82,
        Root = 83,
        Rune = 84,
        SenseTrap = 85,
        Shadowstep = 86,
        Shielding = 87,
        Slow = 88,
        Snare = 89,
        Special = 90,
        SpellFocus = 91,
        SpellGuard = 92,
        Spellshield = 93,
        Stamina = 94,
        ChannelingFocus = 95,
        Strength = 96,
        Stun = 97,
        SumAir = 98,
        SumAnimation = 99,
        SumEarth = 100,
        SumFamiliar = 101,
        SumFire = 102,
        SumUndead = 103,
        SumWarder = 104,
        SumWater = 105,
        SummonArmor = 106,
        SummonFocus = 107,
        SummonFoodWater = 108,
        SummonUtility = 109,
        SummonWeapon = 110,
        Summoned = 111,
        Symbol = 112,
        Taelosia = 113,
        Traps = 114,
        Techniques = 115,
        ThePlanes = 116,
        Timer1 = 117,
        Timer2 = 118,
        Timer3 = 119,
        Timer4 = 120,
        Timer5 = 121,
        Timer6 = 122,
        Transport = 123,
        Undead = 124,
        UtilityBeneficial = 125,
        UtilityDetrimental = 126,
        Velious = 127,
        Visages = 128,
        Vision = 129,
        WisdomIntelligence = 130,
        Traps2 = 131,
        Auras = 132,
        Endurance = 133,
        SerpentsSpine = 134,
        Corruption = 135,
        Learning = 136,
        Chromatic = 137,
        Prismatic = 138,
        SumSwarm = 139,
        Delayed = 140,
        Temporary = 141,
        Twincast = 142,
        SumBodyguard = 143,
        Humanoid = 144,
        HasteSpellFocus = 145
    }

    [Serializable]
    public class SpellBase
    {
        public int id { get; set; } // not used 
        public string name { get; set; } // Victim of the spell  
        public string cast_on_you { get; set; }
        public string cast_on_other { get; set; }
        public SpellTypes type { get; set; }
        public int casttime { get; set; }
        public int recastTime { get; set; }
        public Dictionary<PlayerClasses, int> Classes { get; set; }
        public string spell_fades { get; set; }
        public int buffdurationformula { get; set; }
        public int pvp_buffdurationformula { get; set; }
        public int spell_icon { get; set; }
        public int buffduration { get; set; }
        public int ResistCheck { get; set; }
        public DescrNumber DescrNumber { get; set; }
        public ResistType resisttype { get; set; }
        public SpellType SpellType { get; set; }
    }
}
