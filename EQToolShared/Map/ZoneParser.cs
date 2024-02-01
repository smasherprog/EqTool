using System;
using System.Collections.Generic;
using System.Linq;

namespace EQToolShared.Map
{
    public class NpcSpawnTime
    {
        public string Name { get; set; }
        public TimeSpan RespawnTime { get; set; }
    }

    public class ZoneInfo
    {
        public bool ShowAllMapLevels { get; set; }
        public int ZoneLevelHeight { get; set; }
        public string Name { get; set; }
        public TimeSpan RespawnTime { get; set; }
        public List<NpcSpawnTime> NpcSpawnTimes { get; set; } = new List<NpcSpawnTime>();
        public List<NpcSpawnTime> NpcContainsSpawnTimes { get; set; } = new List<NpcSpawnTime>();
        public List<string> NotableNPCs { get; set; } = new List<string>();
    }

    public static class ZoneParser
    {
        private const string Youhaveentered = "You have entered ";
        private const string Therearenoplayers = "There are no players ";
        private const string Thereare = "There are ";
        private const string Thereis = "There is ";
        private const string Youhaveenteredareapvp = "You have entered an Arena (PvP) area.";
        private const string spaceinspace = "in ";
        public static readonly List<string> KaelFactionMobs = new List<string>() {
            "Bygloirn Omorden",
            "Dagron Stonecutter",
            "Barlek Stonefist",
            "Gragek Mjlorkigar",
            "Kelenek Bluadfeth",
            "Veldern Blackhammer",
            "Kragek Thunderforge",
            "Stoem Lekbar",
            "Bjarorm Mjlorn",
            "Ulkar Jollkarek",
            "Vylleam Vyaeltor",
            "Jaglorm Ygorr",
            "Yeeldan Spiritcaller"
        };
        public static readonly Dictionary<string, string> ZoneNameMapper = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> ZoneWhoMapper = new Dictionary<string, string>();
        public static readonly Dictionary<string, ZoneInfo> ZoneInfoMap = new Dictionary<string, ZoneInfo>();
#if QUARM
        private static bool isProjectQ = true;
#else
        private static bool isProjectQ = false;
#endif


        static ZoneParser()
        {
            ZoneInfoMap.Add("airplane", new ZoneInfo
            {
                Name = "airplane",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Thunder Spirit Princess",
                    "Noble Dojorn",
                    "Protector of Sky",
                    "Gorgalosk",
                    "Keeper of Souls",
                    "The Spiroc Lord",
                    "The Spiroc Guardian",
                    "Bazzt Zzzt",
                    "Sister of the Spire",
                    "Eye of Veeshan"
                },
                RespawnTime = new TimeSpan(8, 0, 0)
            });
            ZoneInfoMap.Add("akanon", new ZoneInfo
            {

                Name = "akanon",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("arena", new ZoneInfo
            {
                Name = "arena",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                },
                RespawnTime = new TimeSpan(0)
            });
            ZoneInfoMap.Add("befallen", new ZoneInfo
            {
                Name = "befallen",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                   "Boondin Babbinsbort","Commander Windstream","An Elf Skeleton","Gynok Moltor","Priest Amiaz","Skeleton Lrodd","The Thaumaturgist"
                },
                RespawnTime = new TimeSpan(0, 19, 0)
            });
            ZoneInfoMap.Add("beholder", new ZoneInfo
            {
                Name = "beholder",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "King Xorbb","Lord Syrkl","Lord Sviir","Lord Soptyvr","SpinFlint","Brahhm","Yymp the Infernal","Qlei","Goblin Alchect"
                },
                RespawnTime = new TimeSpan(0, 6, 0)
            });
            ZoneInfoMap.Add("blackburrow", new ZoneInfo
            {
                Name = "blackburrow",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Lord Elgnub","Master Brewer","Refugee Splitpaw","a gnoll commander","Splitpaw Commander",
                },
                RespawnTime = new TimeSpan(0, 22, 0)
            });
            ZoneInfoMap.Add("burningwood", new ZoneInfo
            {
                Name = "burningwood",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("butcher", new ZoneInfo
            {
                Name = "butcher",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                NpcContainsSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                    {
                        Name = "Guard Armandin",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Belg",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Burr",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Clendin",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Dunil",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Dunn",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Gabel",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Gonin",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Grae",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Haendar",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Haldin",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Hartt",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Kizzburr",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Kyte",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Mandin",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Norin",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Rundul",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Rylin",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Sornn",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Srinn",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Zyburr",
                        RespawnTime = new TimeSpan(0, 24, 0)
                    }
                 },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                      new NpcSpawnTime
                      {
                            Name ="Nyzil Bloodforge",
                            RespawnTime = new TimeSpan(0,6, 40)
                      },
                         new NpcSpawnTime
                      {
                            Name ="Durkis Battlemore",
                            RespawnTime = new TimeSpan(0,6, 40)
                      },
                         new NpcSpawnTime
                      {
                            Name ="Walnan",
                            RespawnTime = new TimeSpan(0,6, 40)
                      },
                         new NpcSpawnTime
                      {
                            Name ="Happ Findlefinn",
                            RespawnTime = new TimeSpan(0,6, 40)
                      },
                         new NpcSpawnTime
                      {
                            Name ="Atwin Keladryn",
                            RespawnTime = new TimeSpan(0,6, 40)
                      },
                         new NpcSpawnTime
                      {
                            Name ="Balen Kalgunn",
                            RespawnTime = new TimeSpan(0,6, 40)
                      },
                         new NpcSpawnTime
                      {
                            Name ="Thar Kelgand",
                            RespawnTime = new TimeSpan(0,6, 40)
                      }
                 },
                RespawnTime = new TimeSpan(0, 15, 0)
            });
            ZoneInfoMap.Add("cabeast", new ZoneInfo
            {
                Name = "cabeast",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Vessel Drozlin",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("cabwest", new ZoneInfo
            {
                Name = "cabwest",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("cauldron", new ZoneInfo
            {
                Name = "cauldron",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("cazicthule", new ZoneInfo
            {
                Name = "cazicthule",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 22, 00)
            });
            ZoneInfoMap.Add("charasis", new ZoneInfo
            {
                Name = "charasis",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Sentient Bile","The Crypt Excavator","The Crypt Feaster","The Crypt Keeper","Drusella Sathir","Embalming Fluid","The Golem Master","Reanimated Plaguebone","Skeletal Procurator","The Skeleton Sepulcher","The Spectre Spiritualist","The Undertaker Lord",
                },
                RespawnTime = new TimeSpan(0, 20, 30),
                NpcSpawnTimes = new List<NpcSpawnTime>
                 {
                     new NpcSpawnTime
                     {
                          Name = "Drusella Sathir",
                          RespawnTime = new TimeSpan(24, 0, 0)
                     }
                 }
            });
            ZoneInfoMap.Add("chardok", new ZoneInfo
            {
                Name = "chardok",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 30,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                      new NpcSpawnTime
                      {
                           Name = "Kennel Master Al`ele",
                            RespawnTime = new TimeSpan(0, 20, 0)
                      },
                      new NpcSpawnTime
                      {
                          Name = "an apprentice kennelmaster",
                        RespawnTime = new TimeSpan(0, 20, 0)
                      }
                 },
                RespawnTime = new TimeSpan(0, 18, 00)
            });
            ZoneInfoMap.Add("citymist", new ZoneInfo
            {
                Name = "citymist",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "a black reaver","an army behemoth","Captain of the Guard","a human skeleton","Lhranc","Lord Ghiosk","Lord Rak`Ashiir","Neh`Ashiir","spectral courier","Wraith of Jaxion",
                },
                RespawnTime = new TimeSpan(0, 22, 00)
            });
            ZoneInfoMap.Add("cobaltscar", new ZoneInfo
            {
                Name = "cobaltscar",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 20, 00)
            });
            ZoneInfoMap.Add("commons", new ZoneInfo
            {
                Name = "commons",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("crushbone", new ZoneInfo
            {
                Name = "crushbone",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Orc Taskmaster","Orc Trainer","Orc Warlord","Ambassador DVinn","Lord Darish","Rondo Dunfire","Retlon Brenclog","Emperor Crush","The Prophet",
                },
                RespawnTime = new TimeSpan(0, 9, 00)
            });
            ZoneInfoMap.Add("crystal", new ZoneInfo
            {
                Name = "crystal",
                ZoneLevelHeight = 20,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 14, 45)
            });
            ZoneInfoMap.Add("dalnir", new ZoneInfo
            {
                Name = "dalnir",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Coerced Channeler","Coerced Crusader","Coerced Penkeeper","a coerced revenant","a coerced smith","Kly Imprecator","The Kly Overseer","The Kly","Spectral Crusader","an undead blacksmith","lumpy goo",
                },
                RespawnTime = new TimeSpan(0, 12, 00)
            });
            ZoneInfoMap.Add("dreadlands", new ZoneInfo
            {
                Name = "dreadlands",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Gorenaire","A dread widow","a mountain giant patriarch","a wulfare lonewolf","wraithbone champion",
                },
                RespawnTime = new TimeSpan(0, 6, 40),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                          Name="A Tundra Yeti",
                          RespawnTime = new TimeSpan(0, 16, 0)
                     },
                     new NpcSpawnTime
                     {
                          Name="A Glacier Yeti",
                          RespawnTime = new TimeSpan(0, 16, 0)
                     }
                 }
            });
            ZoneInfoMap.Add("droga", new ZoneInfo
            {
                Name = "droga",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "a goblin bodyguard","Chief Rokgus","a goblin canyoneer","a maddened Burynai","Soothsayer Dregzak", "An angry goblin", "Warlord Skargus"
                },
                RespawnTime = new TimeSpan(0, 20, 30)
            });
            ZoneInfoMap.Add("eastkarana", new ZoneInfo
            {
                Name = "eastkarana",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 0)
            });
            ZoneInfoMap.Add("eastwastes", new ZoneInfo
            {
                Name = "eastwastes",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Boridain Glacierbane", "Gloradin Coldheart", "Chief Ry`Gorr","Corbin Blackwell","Ekelng Thunderstone","Firbrand the Black","Oracle of Ry'Gorr","Fjloaren Icebane","Garadain Glacierbane","Ghrek Squatnot","Tain Hammerfrost","Warden Bruke"
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("ecommons", new ZoneInfo
            {
                Name = "ecommons",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("emeraldjungle", new ZoneInfo
            {
                Name = "emeraldjungle",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Engorged Soulsipper", "Severilous", "Totem Fiendling",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("erudnext", new ZoneInfo
            {
                Name = "erudnext",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("erudnint", new ZoneInfo
            {
                Name = "erudnint",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("erudsxing", new ZoneInfo
            {
                Name = "erudsxing",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("everfrost", new ZoneInfo
            {
                Name = "everfrost",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Karg IceBear","Lich of Miragul","Megan","Tundra Jack","Iceberg","Snowflake","Sulon McMoor","Redwind","Martar IceBear","Dark Assassin","Corrupted wooly",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("fearplane", new ZoneInfo
            {
                Name = "fearplane",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Fright","Dread", "Terror", "Dracoliche", "Cazic Thule"
                },
                RespawnTime = new TimeSpan(8, 0, 0)
            });
            ZoneInfoMap.Add("feerrott", new ZoneInfo
            {
                Name = "feerrott",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Annaelia Wylassi","Aqaar Aluram","Cyndreela","Dark Assassin","Eleann Morkul","Oknoggin Stonesmacker","Roror","Spanner Scrapsnatcher",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("felwithea", new ZoneInfo
            {
                Name = "felwithea",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("felwitheb", new ZoneInfo
            {
                Name = "felwitheb",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("fieldofbone", new ZoneInfo
            {
                Name = "fieldofbone",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "A scaled prowler","A skeletal jester","Burynaibane Spider","Carrion Queen","Gharg Oberbord","Iksar Dakoit","Jairnel Marfury","Kerosh Blackhand","Targishin","The Tangrin","a burynai cutter","a scourgetail scorpion",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("firiona", new ZoneInfo
            {
                Name = "firiona",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("freporte", new ZoneInfo
            {
                Name = "freporte",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 15,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("freportn", new ZoneInfo
            {
                Name = "freportn",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("freportw", new ZoneInfo
            {
                Name = "freportw",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("frontiermtns", new ZoneInfo
            {
                Name = "frontiermtns",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("frozenshadow", new ZoneInfo
            {
                Name = "frozenshadow",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Large Undead Gnoll","Xalgoti","Zorglim the Dead","Enraged Shadowbeast","Amontehepna","Narmak Berreka","maggot infested flesh","Eugie","Isopca","Lerty","Nosja","Otdd","Pelpa","Priest Majes Medory","Tihgren","Varjie","Vyakna","enraged relative","lucid spirit of Abrams","Vhal'Sera",
                },
                RespawnTime = new TimeSpan(0, 20, 0)
            });
            ZoneInfoMap.Add("gfaydark", new ZoneInfo
            {
                Name = "gfaydark",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 7, 5)
            });
            ZoneInfoMap.Add("greatdivide", new ZoneInfo
            {
                Name = "greatdivide",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Sentry Badain",  "Bekerak Coldbones",  "Blizzent",  "Bloodmaw", "Captain Stonefist",  "Drakkel Blood Wolf",  "Fergul Frostsky",  "Gralk Dwarfkiller","Icetooth",  "Korf Brokenhammer",  "Relik",  "Shardtooth","Shardwurm Broodmother",  "Shardwurm Matriarch",  "a Tizmak Spiritcaller",  "Vluudeen","Vores the Hunter","Yaka Razorhoof"
                },
                RespawnTime = new TimeSpan(0, 10, 40)
            });
            ZoneInfoMap.Add("grobb", new ZoneInfo
            {
                Name = "grobb",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("growthplane", new ZoneInfo
            {
                Name = "growthplane",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Ancient Totem", "Ail the Elder", "Fayl Everstrong", "Farstride Unicorn", "Prince Thirneg", "Galiel Spirithoof", "Grahl Strongback", "A Phase Puma", "Ordro", "Sarik the Fang", "Treah Greenroot", "Undogo Digolo", "Tunare" },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                      new NpcSpawnTime
                      {
                           Name = "a protector of growth",
                            RespawnTime = new TimeSpan(0, 24, 0)
                      },
                    new NpcSpawnTime
                      {
                           Name = "Tunare",
                            RespawnTime = new TimeSpan(0, 0, 1)
                      }
                 },
                RespawnTime = new TimeSpan(12, 0, 0)
            });
            ZoneInfoMap.Add("gukbottom", new ZoneInfo
            {
                Name = "gukbottom",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "a basalt gargoyle", "Raster of Guk", "a frenzied ghoul", "a froglok crusader", "a froglok herbalist", "the froglok king", "a froglok noble", "a yun priest", "a froglok tactician", "the ghoul arch magus", "a ghoul assassin", "a ghoul cavalier", "a ghoul executioner", "the ghoul lord", "a ghoul ritualist", "a ghoul sage", "a ghoul savant", "a ghoul scribe", "a ghoul sentinel", "a ghoul supplier", "a huge water elemental", "a minotaur elder", "a minotaur patriarch", "a reanimated hand", "Slaythe the Slayer" },
                RespawnTime = new TimeSpan(0, 28, 0)
            });
            ZoneInfoMap.Add("guktop", new ZoneInfo
            {
                Name = "guktop",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "a froglok gaz squire","a froglok idealist","a froglok realist","a froglok necromancer","a froglok scryer","a froglok summoner","a froglok nokta shaman","a froglok shin knight","the froglok shin lord","Tempus","a giant heart spider",
                },
                RespawnTime = new TimeSpan(0, 16, 30)
            });
            ZoneInfoMap.Add("halas", new ZoneInfo
            {
                Name = "halas",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("hateplane", new ZoneInfo
            {
                Name = "hateplane",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Ashenbone Broodmaster", "Avatar of Abhorrence", "Coercer T`vala", "Grandmaster R`Tal", "High Priest M`kari", "Lord of Ire", "Lord of Loathing", "Magi P`Tasa", "Master of Spite", "Mistress of Scorn", "Maestro of Rancor", "Innoruuk"
                },
                RespawnTime = new TimeSpan(8, 0, 0)
            });
            ZoneInfoMap.Add("highkeep", new ZoneInfo
            {
                Name = "highkeep",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Captain Boshinko","Mistress Anna","Osargen","Princess Lenia",
                },
                NpcContainsSpawnTimes = new List<NpcSpawnTime>()
                {
                    new NpcSpawnTime
                    {
                        Name = "Goblin",
                        RespawnTime = new TimeSpan(0, 10, 0)
                    }
                },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                {
                    new NpcSpawnTime
                    {
                        Name = "Guard Chopin",
                        RespawnTime = new TimeSpan(0, 3, 0)
                    },
                    new NpcSpawnTime
                    {
                        Name = "Guard Blayle",
                        RespawnTime = new TimeSpan(0, 3, 0)
                    },
                     new NpcSpawnTime
                    {
                        Name = "A noble",
                        RespawnTime = new TimeSpan(0, 6, 0)
                    },
                     new NpcSpawnTime
                    {
                        Name = "Isabella Cellus",
                        RespawnTime = new TimeSpan(0, 6, 0)
                    },
                     new NpcSpawnTime
                    {
                        Name = "Captain Boshinko",
                        RespawnTime = new TimeSpan(0, 6, 0)
                    }
                },
                RespawnTime = new TimeSpan(0, 18, 0)
            });
            ZoneInfoMap.Add("highpass", new ZoneInfo
            {
                Name = "highpass",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Cyrla Shadowstepper","Dyllin Starsine","Hagnis Shralok","Recfek Shralok","Vopuk Shralok","Vexven Mucktail","Grenix Mucktail","Barn Bloodstone",
                },
                RespawnTime = new TimeSpan(0, 5, 0),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                         Name = "Hagnis Shralok",
                         RespawnTime = new TimeSpan(0, 22, 0)
                     },
                      new NpcSpawnTime
                     {
                         Name = "Recfek Shralok",
                         RespawnTime = new TimeSpan(0, 22, 0)
                     },
                      new NpcSpawnTime
                     {
                         Name = "Vopuk Shralok",
                         RespawnTime = new TimeSpan(0, 22, 0)
                     },
                      new NpcSpawnTime
                     {
                         Name = "Vexven Mucktail",
                         RespawnTime = new TimeSpan(0, 22, 0)
                     },
                      new NpcSpawnTime
                     {
                         Name = "Grenix Mucktail",
                         RespawnTime = new TimeSpan(0, 22, 0)
                     }
                 }
            });
            ZoneInfoMap.Add("hole", new ZoneInfo
            {
                Name = "hole",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Bejeweled Elemental","Commander Yarik","Caradon","Dartain the Lost","Ghost of Kindle","Ghost of Glohnor","Gibartik","High Scale Kirn","Initiate Sirlis","Irslak the Wretched","Jaeil the Wretched","Keeper of the Tombs","Kejar the Mighty","Master Yael","Niltoth the Unholy","Nortlav the Scalekeeper","Polzin Mrid","a ratman guard","Rocksoul","Schnozz the Flighty","Stonegrinder Minion","Stonesoul the Unmoving","Ulrik the Devout",
                },
                RespawnTime = new TimeSpan(0, 21, 30)
            });
            ZoneInfoMap.Add("iceclad", new ZoneInfo
            {
                Name = "iceclad",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Balix Misteyes","Corudoth","Garou","Lodizal","Midnight","Pulsating Icestorm","Stormfeather",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("innothule", new ZoneInfo
            {
                Name = "innothule",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Bunk Oden","Jojoojojgogogoguna","Jyle Windstorm","Jojongua","Zimbittle","a troll slayer","Spore Guardian","Lynuga",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kael", new ZoneInfo
            {
                Name = "kael",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "The Avatar of War","The Statue of Rallos Zek","Derakor the Vindicator","King Tormax","Bjrakor the Cold","Captain Bvellos","Gkrean Prophet of Tallon","Semkak Prophet of Vallon","Gorul Longshanks","Keldor Dek`Torek","Noble Helssen","Slaggak the Trainer","Staff Sergeant Drioc","Vkjor","Wenglawks Kkeak",
                },
                RespawnTime = new TimeSpan(0, 28, 0),
                NpcSpawnTimes = new List<NpcSpawnTime>
                 {
                     new NpcSpawnTime
                     {
                        Name = "Keldor Dek`Torek",
                        RespawnTime = new TimeSpan(18, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Derakor the Vindicator",
                        RespawnTime = new TimeSpan(7, 0, 0)
                     }
                 }
            });
            ZoneInfoMap.Add("kaesora", new ZoneInfo
            {
                Name = "kaesora",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Hungered Ravener","failed crypt raider","Frenzied Strathbone","Reaver of Xalgoz","spectral guardian","spectral librarian","Strathbone Runelord","tortured librarian","Warder of Xalgoz","Xalgoz",
                },
                RespawnTime = new TimeSpan(0, 18, 0)
            });
            ZoneInfoMap.Add("kaladima", new ZoneInfo
            {
                Name = "kaladima",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kaladimb", new ZoneInfo
            {
                Name = "kaladimb",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("karnor", new ZoneInfo
            {
                Name = "karnor",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Caller of Sathir", "A construct", "Construct of Sathir", "a cursed hand", "a drolvarg captain", "A Drolvarg Pawbuster", "a Drolvarg warlord", "Hangnail", "a human skeleton", "Knight of Sathir", "Sentry of Sathir", "Skeletal Berserker", "Skeletal Captain", "Skeletal Caretaker", "Skeletal Scryer", "skeletal warlord", "Spectral Turnkey", "Undead Jailor", "Venril Sathir", "Venril Sathir Remains", "Verix Kyloxs Remains" },
                RespawnTime = new TimeSpan(0, 27, 0)
            });
            ZoneInfoMap.Add("kedge", new ZoneInfo
            {
                Name = "kedge",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Phinigel Autropos","Cauldronboil","Cauldronbubble","Coralyn Kelpmaiden","Estrella of Gloomwater","Fierce Impaler","a ferocious cauldron shark","Frenzied Cauldron Shark","Golden Haired Mermaid","Stiletto Fang Piranha","Seahorse Patriarch","Seahorse Matriarch","Shellara Ebbhunter","Undertow","Swirlspine",
                },
                RespawnTime = new TimeSpan(0, 22, 0)
            });
            ZoneInfoMap.Add("kerraridge", new ZoneInfo
            {
                Name = "kerraridge",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 17, 45)
            });
            ZoneInfoMap.Add("kithicor", new ZoneInfo
            {
                Name = "kithicor",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Ged Twigborn", "Mildin Whistler", "Chief Gan`Shralok", "Recfek Shralok", "and the other Shralok Orcs", "Thumper", "Irin Lunis", "Leaf Falldim", "Kithicor", "Eenot", "Kobb", "Giz X`Tin" },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kurn", new ZoneInfo
            {
                Name = "kurn",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Bargynn","Burynai Forager","fingered skeleton","an odd mole","a skeletal cook","thick boned skeleton","undead crusader","an undead jester",
                },
                RespawnTime = new TimeSpan(0, 18, 20)
            });
            ZoneInfoMap.Add("lakeofillomen", new ZoneInfo
            {
                Name = "lakeofillomen",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "a sarnak courier","Professor Akabao","Chancellor of Di`Zok","Lord Gorelik","Advisor Sh'Orok",
                },
                RespawnTime = new TimeSpan(0, 6, 40),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                      new NpcSpawnTime
                      {
                           Name = "a bloodgill goblin",
                            RespawnTime = new TimeSpan(0, 13, 0)
                      },
                       new NpcSpawnTime
                      {
                           Name = "Chancellor of Di`Zok",
                            RespawnTime = new TimeSpan(2, 0, 0)
                      }
                 }
            });
            ZoneInfoMap.Add("lakerathe", new ZoneInfo
            {
                Name = "lakerathe",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 0)
            });
            ZoneInfoMap.Add("lavastorm", new ZoneInfo
            {
                Name = "lavastorm",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Deep Lava Basilisk","Eejag","Hykallen","A lesser nightmare","Sir Lindeal","a warbone monk","a warbone spearman",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("lfaydark", new ZoneInfo
            {
                Name = "lfaydark",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("mischiefplane", new ZoneInfo
            {
                Name = "mischiefplane",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(1, 10, 10)
            });
            ZoneInfoMap.Add("mistmoore", new ZoneInfo
            {
                Name = "mistmoore",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "an advisor","an avenging caitiff","Black Dire","Butler Syncall","a cloaked dhampyre","a deathly usher","Enynti","Garton Viswin","a glyphed ghoul","an imp familiar","Lasna Cheroon","Maid Issis","Mayong Mistmoore","Mynthi Davissi","Princess Cherista","Ssynthi","Xicotl",
                },
                RespawnTime = new TimeSpan(0, 22, 00),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                {
                    new NpcSpawnTime
                    {
                         Name = "an Advisor",
                         RespawnTime = new TimeSpan(4, 0, 0)
                    }
                }
            });
            ZoneInfoMap.Add("misty", new ZoneInfo
            {
                Name = "misty",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("najena", new ZoneInfo
            {
                Name = "najena",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>()
                {
                    "Akksstaff","BoneCracker","Drelzna","Ekeros","Linara Parlone","Moosh","Najena","Officer Grush","Rathyl","Rathyl reincarnate","Trazdon","a visiting priestess","The Widowmistress",
                },
                RespawnTime = new TimeSpan(0, 19, 0)
            });
            ZoneInfoMap.Add("necropolis", new ZoneInfo
            {
                Name = "necropolis",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Zlandicar", "A Paebala Spirit Talker", "Dominator Yisaki", "Garzicor's Wraith", "Jaled Dar`s Shade", "Neb", "Queen Raltaas", "Seeker Bulava", "Vaniki", "Vilefang", "Warmaster Utvara" },
                RespawnTime = new TimeSpan(0, 27, 00)
            });
            ZoneInfoMap.Add("nektulos", new ZoneInfo
            {
                Name = "nektulos",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Leatherfoot Deputy", "Leatherfoot Medic", "Kirak Vil", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("neriaka", new ZoneInfo
            {
                Name = "neriaka",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                      new NpcSpawnTime
                      {
                            Name = "Jacker",
                            RespawnTime =  new TimeSpan(0, 8, 0)
                      },
                     new NpcSpawnTime
                      {
                            Name = "Karnan",
                            RespawnTime =  new TimeSpan(0, 8, 0)
                      },
                                new NpcSpawnTime
                      {
                            Name = "Uglan",
                            RespawnTime =  new TimeSpan(0, 6, 40)
                      },
                    new NpcSpawnTime
                      {
                            Name = "Mrak",
                            RespawnTime =  new TimeSpan(0, 8, 0)
                      },
                    new NpcSpawnTime
                      {
                            Name = "Capee",
                            RespawnTime =  new TimeSpan(0, 6, 40)
                      },
                    new NpcSpawnTime
                      {
                            Name = "Svunsa",
                            RespawnTime =  new TimeSpan(0, 6, 40)
                      }

                 },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("neriakb", new ZoneInfo
            {
                Name = "neriakb",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("neriakc", new ZoneInfo
            {
                Name = "neriakc",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 24, 0)
            });
            ZoneInfoMap.Add("northkarana", new ZoneInfo
            {
                Name = "northkarana",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Xanuusus", "Ashenpaw", "Zahal the Vile", "GrimFeather", "Swiftclaw", "Lieutenant Midraim", "The Silver Griffon", "Timbur the Tiny", "Korvik the Cursed", },
                RespawnTime = new TimeSpan(0, 6, 0)
            });
            ZoneInfoMap.Add("nro", new ZoneInfo
            {
                Name = "nro",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Dorn B`Dynn", "Dunedigger", "Rahotep", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("nurga", new ZoneInfo
            {
                Name = "nurga",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Overseer Dlubish", "A Sleeping Ogre", "Trunt", },
                RespawnTime = new TimeSpan(0, 20, 30)
            });
            ZoneInfoMap.Add("oasis", new ZoneInfo
            {
                Name = "oasis",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Cazel", "Hatar", "Lockjaw", "Young Ronin", },
                RespawnTime = new TimeSpan(0, 6, 40),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                         Name = "A Spectre",
                          RespawnTime = new TimeSpan(0, 16, 30)
                     }
                 }
            });
            ZoneInfoMap.Add("oggok", new ZoneInfo
            {
                Name = "oggok",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 24, 00)
            });
            ZoneInfoMap.Add("oot", new ZoneInfo
            {
                Name = "oot",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Capt Surestout", "Nerbilik", "Oracle of K`Arnon", "Boog Mudtoe", "Gornit", "Sentry Xyrin", "Gull Skytalon", "Allizewsaur", "Ancient Cyclops", "Brawn", "Quag Maelstrom", "Seplawishinl Bladeblight", "Soarin Brightfeather", "tainted seafury cyclops", "corrupted seafury cyclops", "Wiltin Windwalker", "A Goblin", },
                RespawnTime = new TimeSpan(0, 6, 0),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                          Name = "Guardian of K`Arnon",
                           RespawnTime = new TimeSpan(0, 22, 0)
                     },
                       new NpcSpawnTime
                     {
                          Name = "Gull Skytalon",
                           RespawnTime = new TimeSpan(8, 0, 0)
                     }
                 }
            });
            ZoneInfoMap.Add("overthere", new ZoneInfo
            {
                Name = "overthere",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Admiral Tylix", "Captain Rottgrime", "General V`Deers", "Impaler Tzilug", "Tourmaline", "Corundium", "Stishovite", "Tektite", "A Cliff Golem", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("paineel", new ZoneInfo
            {
                Name = "paineel",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                    new NpcSpawnTime
                     {
                          Name = "Guard Ackin",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Captain Latorl",
                          RespawnTime = new TimeSpan(0, 24, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Hetorzuz",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Ishvlor",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Korlack",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Lehlufa",
                          RespawnTime = new TimeSpan(0, 6, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Lecknar",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Mertanor",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Lesunra",
                          RespawnTime = new TimeSpan(0, 6, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Pendleir",
                          RespawnTime = new TimeSpan(0, 6, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Perelin",
                          RespawnTime = new TimeSpan(0, 6, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Yerlash",
                          RespawnTime = new TimeSpan(0, 6, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Polzdurn",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Pomnares",
                          RespawnTime = new TimeSpan(0, 6, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Potren",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Sheltuin",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Tynaryn",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                    new NpcSpawnTime
                     {
                          Name = "Guard Yerlash",
                          RespawnTime = new TimeSpan(0, 25, 0)
                     },
                 },
                RespawnTime = new TimeSpan(0, 10, 30)
            });
            ZoneInfoMap.Add("paw", new ZoneInfo
            {
                Name = "paw",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Brother Hayle", "The Ishva Mal", "Kurrpok Splitpaw", "Tesch Val Kadvem", "Tesch Val Deval`Nmak", "Nisch Val Torash Mashk", "Rosch Val L'Vlor" },
                RespawnTime = new TimeSpan(0, 22, 0),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                           Name = "The Ishva Mal",
                            RespawnTime = new TimeSpan(0, 28, 0)
                     },
                      new NpcSpawnTime
                     {
                           Name = "An Ishva Lteth gnoll",
                            RespawnTime = new TimeSpan(0, 28, 0)
                     }
                 }
            });
            ZoneInfoMap.Add("permafrost", new ZoneInfo
            {
                Name = "permafrost",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Lady Vox", "Priest of Nagafen", "High Priest Zaharn", "A goblin alchemist (Permafrost)", "King Thex'Ka IV", "Goblin Archeologist", "Goblin Patriarch", "Goblin Preacher", "Goblin Jail Master", "Goblin Scryer", "Elite Honor Guard", "Injured Polar Bear", "Ice Goblin Champion", "Ice Giant Diplomat", },
                RespawnTime = new TimeSpan(0, 22, 0)
            });
            ZoneInfoMap.Add("qcat", new ZoneInfo
            {
                Name = "qcat",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 12, 0),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                {
                    new NpcSpawnTime
                    {
                        Name = "A Spectre",
                         RespawnTime = new TimeSpan(0, 24, 0)
                    }
                }
            });
            ZoneInfoMap.Add("qey2hh1", new ZoneInfo
            {
                Name = "qey2hh1",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("qeynos", new ZoneInfo
            {
                Name = "qeynos",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("qeynos2", new ZoneInfo
            {
                Name = "qeynos2",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("qeytoqrg", new ZoneInfo
            {
                Name = "qeytoqrg",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("qrg", new ZoneInfo
            {
                Name = "qrg",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("rathemtn", new ZoneInfo
            {
                Name = "rathemtn",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Ankhefenmut", "Findlegrob", "Sindlegrob", "Bindlegrob", "Blackwing", "Broog Bloodbeard", "Brother Zephyl", "Grazhak the Berserker", "Hasten Bootstrutter", "Maldyn the Unkempt", "monstrous zombie", "Mortificator Syythrak", "Oculys Ogrefiend", "Petrifin", "Quid Rilstone", "Rharzar", "Shardwing", "Tarskuk", "Theodast Wuggmump", "Zazamoukh" },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("rivervale", new ZoneInfo
            {
                Name = "rivervale",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Ankhefenmut", "Findlegrob", "Sindlegrob", "Bindlegrob", "Blackwing", "Broog Bloodbeard", "Brother Zephyl", "Grazhak the Berserker", "Hasten Bootstrutter", "Maldyn the Unkempt", "monstrous zombie", "Mortificator Syythrak", "Oculys Ogrefiend", "Petrifin", "Quid Rilstone", "Rharzar", "Shardwing", "Tarskuk", "Theodast Wuggmump", "Zazamoukh" },
                RespawnTime = new TimeSpan(0, 22, 0)
            });
            ZoneInfoMap.Add("runnyeye", new ZoneInfo
            {
                Name = "runnyeye",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Borxx", "an Evil Eye prisoner", "Sludge Dankmire", "A Goblin Captain", "Goblin Warlord", "The Goblin King", "Slime Elemental", "Gelatinous Cube", },
                RespawnTime = new TimeSpan(0, 22, 0)
            });
            ZoneInfoMap.Add("sebilis", new ZoneInfo
            {
                Name = "sebilis",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Arch Duke Iatol", "Baron Yosig", "blood of chottal", "Brogg", "crypt caretaker", "Emperor Chottal", "frenzied pox scarab", "Froggy", "froglok armorer", "froglok armsman", "froglok chef", "froglok commander", "froglok ostiary", "froglok pickler", "froglok repairer", "Gangrenous scarab", "Gruplinort", "Harbinger Freglor", "Hierophant Prime Grekal", "myconid spore king", "a necrosis scarab", "sebilite protector", "Tolapumj", "Trakanon", },
                RespawnTime = new TimeSpan(0, 27, 0),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                      new NpcSpawnTime
                      {
                           Name = "Tolapumj",
                            RespawnTime = new TimeSpan(2, 45, 0)
                      },
                       new NpcSpawnTime
                      {
                           Name = "Sebilite protector",
                           RespawnTime = new TimeSpan(2, 45, 0)
                      }
                 }
            });
            ZoneInfoMap.Add("sirens", new ZoneInfo
            {
                Name = "sirens",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Elna Kelpweaver", "Fellspine", "Helsia Mindreaver", "Mistress Latazura", "Priestess Sercema", "Shimmering Sea Spirit", "Ulth the Enraged" },
                RespawnTime = new TimeSpan(0, 28, 0)
            });
            ZoneInfoMap.Add("skyfire", new ZoneInfo
            {
                Name = "skyfire",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Black Scar", "Eldrig the Old", "Faerie of Dismay", "Felia Goldenwing", "Guardian of Felia", "Jennus Lyklobar", "a lava walker", "a shadow drake", "a soul devourer", "Talendor", "a wandering wurm", "a wurm spirit", },
                RespawnTime = new TimeSpan(0, 13, 0)
            });
            ZoneInfoMap.Add("skyshrine", new ZoneInfo
            {
                Name = "skyshrine",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Ziglark Whisperwing", "Lord Yelinak" },
                RespawnTime = new TimeSpan(0, 30, 0)
            });
            ZoneInfoMap.Add("sleeper", new ZoneInfo
            {
                Name = "sleeper",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Kerafyrm", "The Progenitor", "The Final Arbiter", "Master of the Guard", "Hraashna the Warder", "Nanzata the Warder", "Tukaarak the Warder", "Ventani the Warder" },
                RespawnTime = new TimeSpan(8, 0, 0)
            });
            ZoneInfoMap.Add("soldunga", new ZoneInfo
            {
                Name = "soldunga",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Captain Bipnubble", "CWG Model EXG", "Fire Goblin Bartender", "Inferno Goblin Captain", "Fire Goblin Drunkard", "Goblin High Shaman", "Solusek Goblin King", "Gabbie Mardoddle", "flame goblin foreman", "Inferno Goblin Torturer", "Kindle", "Kobold predator", "lava elemental", "Lord Gimblox", "Lynada the Exiled", "Marfen Binkdirple", "Reckless Efreeti", "Singe", },
                RespawnTime = new TimeSpan(0, 18, 0)
            });
            ZoneInfoMap.Add("soldungb", new ZoneInfo
            {
                Name = "soldungb",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "death beetle", "Efreeti Lord Djarn", "guano harvester", "King Tranix", "kobold champion", "kobold noble", "kobold priest", "Lord Nagafen", "Midghh the Dark", "Magi Rokyl", "noxious spider", "Solusek kobold king", "stone spider", "Targin the Rock", "Warlord Skarlon", "Zordak Ragefire" },
                RespawnTime = new TimeSpan(0, 22, 0)
            });
            ZoneInfoMap.Add("soltemple", new ZoneInfo
            {
                Name = "soltemple",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 5, 0)
            });
            ZoneInfoMap.Add("southkarana", new ZoneInfo
            {
                Name = "southkarana",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Baenar Swiftsong", "Brother Drash", "Brother Qwinn", "Coloth Meadowgreen", "Cracktusk", "Ghanex Drah", "Gnashmaw", "Gnawfang", "Grizzleknot", "Groi Gutblade", "Knari Morawk", "Krak Windchaser", "Kroldir Thunderhoof", "Lord Grimrot", "Marik Clubthorn", "Mroon", "Narra Tanith", "Quillmane", "Sentry Alechin", "Synger Foxfyre", "Undead Cyclops", "Vhalen Nostrolo" },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                          Name = "High Shaman Phido",
                          RespawnTime = new TimeSpan(0, 22, 0)
                     },
                         new NpcSpawnTime
                     {
                          Name = "a treant",
                          RespawnTime = new TimeSpan(0, 4, 0)
                     }
                 },
                RespawnTime = new TimeSpan(0, 6, 0)
            });
            ZoneInfoMap.Add("sro", new ZoneInfo
            {
                Name = "sro",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Ancient Cyclops", "Erg Bluntbruiser", "Ortallius", "Rathmana Allin", "Sandgiant Husam", "Scrounge", "Terrorantula", "Young Ronin", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("steamfont", new ZoneInfo
            {
                Name = "steamfont",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Feddi Dooger", "A Kobold Missionary", "Meldrath The Malignant", "Minotaur Hero", "Minotaur Lord", "Renux Herkanor", "Nilit's contraption", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("stonebrunt", new ZoneInfo
            {
                Name = "stonebrunt",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Arglar the Tormentor", "Ghost of Ridossan", "Giang Yin", "a heretic invader", "Hurglak the Destroyer", "Mirabai", "Miranda", "Khonza Ayssla", "Mrowro Wirewhisker", "Old Ghostback", "Prowler of the Jungle", "Rognarog the Infuriated", "Saemey Wirewhisker", "Slyder the Ancient", "Snowbeast" },
                RespawnTime = new TimeSpan(0, 11, 10)
            });
            ZoneInfoMap.Add("swampofnohope", new ZoneInfo
            {
                Name = "swampofnohope",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Bloodgorge", "an escaped froglok", "Deadeye", "Dreesix Ghoultongue", "Dugroz", "Fakraa the Forsaken", "Fangor", "Frayk", "Froglok Repairer", "Froszik the Impaler", "Grik the Exiled", "Grimewurm", "Grizshnok", "Soblohg", "Two Tails", "Ulump Pujluk", "Venomwing", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("templeveeshan", new ZoneInfo
            {
                Name = "templeveeshan",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Casalen", "Dozekar the Cursed", "Essedera", "Grozzmel", "Krigara", "Lepethida", "Midayor", "Tavekalem", "Ymmeln", "Gozzrem", "Lendiniara the Keeper", "Telkorenar", "Cekenar", "Dagarn the Destroyer", "Eashen of the Sky", "Ikatiar the Venom", "Jorlleag", "Lady Mirenilla", "Lady Nevederia", "Lord Feshlak", "Lord Koi'Doken", "Lord Kreizenn", "Lord Vyemm", "Sevalak", "Vulak`Aerr", "Zlexak", },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                        Name = "A crimson claw hatchling",
                        RespawnTime = new TimeSpan(0, 6, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "A shard wyvern hatchling",
                        RespawnTime = new TimeSpan(0, 6, 0)
                     },
                       new NpcSpawnTime
                     {
                        Name = "A skyseeker hatchling",
                        RespawnTime = new TimeSpan(0, 6, 0)
                     },
                        new NpcSpawnTime
                     {
                        Name = "An ebon wing hatchling",
                        RespawnTime = new TimeSpan(0, 6, 0)
                     },
                        new NpcSpawnTime
                     {
                        Name = "An emerald eye hatchling",
                        RespawnTime = new TimeSpan(0, 6, 0)
                     }
                 },
                RespawnTime = new TimeSpan(1, 12, 00)
            });
            ZoneInfoMap.Add("thurgadina", new ZoneInfo
            {
                Name = "thurgadina",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 7, 00)
            });
            ZoneInfoMap.Add("thurgadinb", new ZoneInfo
            {
                Name = "thurgadinb",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Dain Frostreaver IV,", "Glucose", "Grizznot" },
                RespawnTime = new TimeSpan(0, 7, 00)
            });
            ZoneInfoMap.Add("timorous", new ZoneInfo
            {
                Name = "timorous",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Faydedar", "The Great Oowomp", "Halara", "an Iksar master", "Ugrak da Raider", "Xiblin Fizzlebik", "Alrik Farsight" },
                RespawnTime = new TimeSpan(0, 12, 0)
            });
            ZoneInfoMap.Add("tox", new ZoneInfo
            {
                Name = "tox",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "", },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("trakanon", new ZoneInfo
            {
                Name = "trakanon",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Bloodeye", "Champion Arlek", "Champion Thenrin", "Commander Sils", "Crusader Zoglic", "Doom", "Dragontail", "Dreadlord Fanrik", "Ebon Lotus", "Emperor Ganak", "Fallen Iksar", "Ffroaak", "froglok forager", "froglok hunter", "Hangman", "Harbinger Dronik", "Harbinger Josk", "A human skeleton", "Klok Denris", "Knight Dragol", "Pained Soul", "Sigra", "Silvermane", "Squire Glik", "Stonebeak", "Throkkok", "Thruke", "Titail Sinok", "Trakanasaurus Rex", "Vessel Fryn" },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("unrest", new ZoneInfo
            {
                Name = "unrest",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Garanel Rucksif", "a priest of najena", "Khrix Fritchoff", "Khrix's Abomination", "Torklar Battlemaster", "Shadowpincer", "reclusive ghoul magus", },
                RespawnTime = new TimeSpan(0, 22, 0)
            });

            ZoneInfoMap.Add("veeshan", new ZoneInfo
            {
                Name = "veeshan",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Druushk", "Hoshkar", "Nexona", "Phara Dar", "Silverwing", "Xygoz" },
                RespawnTime = new TimeSpan(1, 12, 0)
            });
            ZoneInfoMap.Add("velketor", new ZoneInfo
            {
                Name = "velketor",
                ShowAllMapLevels = false,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Lord Doljonijiarnimorinar", "Velketor the Sorcerer", "A Frenzied Velium Broodling", "A Frenzied Velium Stalker", "Bled", "Bledrek", "Crystal Eyes", "Crystal Fang", "Errkak Icepaw", "Failed Experiment", "Gregendek Icepaw", "Jelek Icepaw", "Kalik Icepaw", "Kerd", "Kerdelb", "Khelkar Icepaw", "Laryk Icepaw", "Leljemor", "Marlek Icepaw", "Meljemor", "Rijoely", "Rowwek Icepaw", "The Brood Master", "The Brood Mother", "Tijoely", "Tpos Icepaw", "Ular Icepaw", "Velketor's Experiment", "Venar Icepaw" },
                RespawnTime = new TimeSpan(0, 32, 50)
            });
            ZoneInfoMap.Add("wakening", new ZoneInfo
            {
                Name = "wakening",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Wuoshi", "A Storm Giant Foreman", "A storm giant surveyor", "Corrupted Faun", "Corrupted Panther", "Countess Silveana", "Eysa Florawhisper", "Frostgiant Overseer", "Grand Vizier Poolakacha`tei", "Korzak Stonehammer", "Lady Gelistial", "Lieutenant Krofer", "Lord Gossimerwind", "Lord Prismwing", "Phenocryst", "Priest Bjek", "Priest Delar", "Priest Grenk", "Rolandal", "Shamus Aghllsews", "a corrupted unicorn", "a storm giant architect" },
                RespawnTime = new TimeSpan(0, 6, 40),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                      new NpcSpawnTime
                      {
                          Name = "A storm giant surveyor",
                           RespawnTime = new TimeSpan(0, 7, 0)
                      },
                       new NpcSpawnTime
                      {
                          Name = "A tar goo",
                           RespawnTime = new TimeSpan(0, 14, 30)
                      },
                       new NpcSpawnTime
                      {
                          Name = "A suit of sentient armor",
                          RespawnTime = new TimeSpan(0, 14, 30)
                      },
                       new NpcSpawnTime
                      {
                          Name = "A faerie dragon",
                          RespawnTime = new TimeSpan(0, 7, 0)
                      }
                 }
            });
            ZoneInfoMap.Add("warrens", new ZoneInfo
            {
                Name = "warrens",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Cave Bat Lord", "Foodmaster Rargnar", "Grodl Ripclaw", "High Shaman Drogik", "Huntmaster Furgrl", "Jailer Mkrarrg", "King Gragnar", "a kobold fisherman", "Krode the Diviner", "Lorekeeper Roggik", "The Mighty Bear Paw", "Muglwump", "Packmaster Dledsh", "Prince Bragnar", "Smithy Rrarrgin", "Trainer Daxgrr", "Warlord Drrig", "Grodl Ripclaw" },
                RespawnTime = new TimeSpan(0, 6, 40),
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                         Name = "King Gragnar",
                         RespawnTime = new TimeSpan(0, 48, 0)
                     },
                      new NpcSpawnTime
                     {
                         Name = "High Shaman Drogik",
                         RespawnTime = new TimeSpan(0, 48, 0)
                     },
                       new NpcSpawnTime
                     {
                         Name = "Lorekeeper Roggik",
                         RespawnTime = new TimeSpan(0, 48, 0)
                     },
                        new NpcSpawnTime
                     {
                         Name = "Trainer Daxgrr",
                         RespawnTime = new TimeSpan(0, 20, 0)
                     },
                         new NpcSpawnTime
                     {
                         Name = "Cave Bat Lord",
                         RespawnTime = new TimeSpan(0, 48, 0)
                     },
                          new NpcSpawnTime
                     {
                         Name = "Foodmaster Rargnar",
                         RespawnTime = new TimeSpan(0, 20, 0)
                     },
                    new NpcSpawnTime
                     {
                         Name = "Huntmaster Furgrl",
                         RespawnTime = new TimeSpan(0, 48, 0)
                     },
                    new NpcSpawnTime
                     {
                         Name = "The Muglwump",
                         RespawnTime = new TimeSpan(0, 35, 0)
                     },
                      new NpcSpawnTime
                     {
                         Name = "Packmaster Dledsh",
                         RespawnTime = new TimeSpan(0, 16, 0)
                     },
                        new NpcSpawnTime
                     {
                         Name = "Prince Bragnar",
                         RespawnTime = new TimeSpan(0, 57, 0)
                     },
                          new NpcSpawnTime
                     {
                         Name = "Smithy Rrarrgin",
                         RespawnTime = new TimeSpan(0, 20, 0)
                     },
                            new NpcSpawnTime
                     {
                         Name = "Warlord Drrig",
                         RespawnTime = new TimeSpan(0, 13, 0)
                     },
                 }
            });
            ZoneInfoMap.Add("warslikswood", new ZoneInfo
            {
                Name = "warslikswood",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "A shady goblin", "Grachnist the Destroyer", "Pit Fighter Dob", "Ssolet Dnaas", "Iksar Knight", "Iksar Bandit Lord" },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("westwastes", new ZoneInfo
            {
                Name = "westwastes",
                ShowAllMapLevels = true,
                ZoneLevelHeight = 10,
                NotableNPCs = new List<string>() { "Atpaev", "Ayillish", "Bratavar", "Bufa", "Cargalia", "Del Sapara", "Derasinal", "Draazak", "Entariz", "Esorpa of the Ring", "Gafala", "Gangel", "Glati", "Harla Dar", "Hechaeva", "Honvar", "Ionat", "Jen Sapara", "Kar Sapara", "Karkona", "Klandicar", "Linbrak", "Makala", "Mazi", "Melalafen", "Myga", "Neordla", "Nintal", "Onava", "Pantrilla", "Quoza", "Sivar", "Sontalak", "Uiliak", "Vitaela", "Von", "Vraptin", "Yal", "Yeldema", "Zil Sapara", "Icehackle", "Makil Rargon", "Mraaka", "Scout Charisa", "Strong Horn", "Tantor", "The Dragon Sage", "Tranala", "Tsiraka", "a Kromzek Captain" },
                NpcSpawnTimes = new List<NpcSpawnTime>()
                 {
                     new NpcSpawnTime
                     {
                        Name = "An elder wyvern",
                        RespawnTime = new TimeSpan(0, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Amcilla",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Atpaev",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Bufa",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Crial",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Gangel",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Honvar",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Linbrak",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Makala",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Onava",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Quoza",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Sivar",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Uiliak",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Yal",
                        RespawnTime = new TimeSpan(8, 30, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Ayillish",
                        RespawnTime = new TimeSpan(8, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Derasinel",
                        RespawnTime = new TimeSpan(8, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Draazak",
                        RespawnTime = new TimeSpan(8, 00, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Entariz",
                        RespawnTime = new TimeSpan(8, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Harla Dar",
                        RespawnTime = new TimeSpan(3, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Ionat",
                        RespawnTime = new TimeSpan(8, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Jen Sapara",
                        RespawnTime = new TimeSpan(8, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Kar Sapara",
                        RespawnTime = new TimeSpan(8, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Karkona",
                        RespawnTime = new TimeSpan(8, 0, 0)
                     },
                      new NpcSpawnTime
                     {
                        Name = "Lord Gossimerwind",
                        RespawnTime = new TimeSpan(0, 6, 40)
                     },
                      new NpcSpawnTime
                     {
                        Name = "A Faerie Dragon",
                        RespawnTime = new TimeSpan(0, 6, 40)
                     }
                 },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            // add customer timers here for PQ
            if (isProjectQ)
            {
                ZoneInfoMap["unrest"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["mistmoore"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["najena"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["blackburrow"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["gukbottom"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["guktop"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["highkeep"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["kedge"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["paw"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["permafrost"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["soldunga"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["soldungb"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["southkarana"].RespawnTime = new TimeSpan(0, 6, 0);
                ZoneInfoMap["runnyeye"].RespawnTime = new TimeSpan(0, 8, 0);
                ZoneInfoMap["cazicthule"].RespawnTime = new TimeSpan(0, 8, 0);
            }

            ZoneWhoMapper.Add("kael drakkal", "kael drakkel");
            ZoneWhoMapper.Add("eastern wastes", "eastern wastelands");
            ZoneWhoMapper.Add("the wakening land", "the wakening lands");
            ZoneWhoMapper.Add("siren's grotto", "sirens grotto");
            ZoneWhoMapper.Add("the plane of hate", "plane of hate");
            ZoneWhoMapper.Add("burning woods", "the burning wood");
            ZoneWhoMapper.Add("south ro", "southern desert of ro");
            ZoneWhoMapper.Add("plane of sky", "plane of air");
            ZoneWhoMapper.Add("south karana", "southern plains of karana");
            ZoneWhoMapper.Add("sleeper's tomb", "sleepers tomb");
            ZoneWhoMapper.Add("mountains of rathe", "rathe mountains");
            ZoneWhoMapper.Add("nektulos forest", "the nektulos forest");
            ZoneWhoMapper.Add("city of mist", "the city of mist");
            ZoneWhoMapper.Add("west cabilis", "cabilis west");
            ZoneWhoMapper.Add("east cabilis", "cabilis east");
            ZoneWhoMapper.Add("cazic-thule", "lost temple of cazic-thule");
            ZoneWhoMapper.Add("everfrost peaks", "everfrost");
            ZoneWhoMapper.Add("highkeep", "high keep");
            ZoneWhoMapper.Add("kithicor forest", "kithicor woods");
            ZoneWhoMapper.Add("lower guk", "ruins of old guk");
            ZoneWhoMapper.Add("north ro", "northern desert of ro");
            ZoneWhoMapper.Add("north karana", "northern plains of karana");
            ZoneWhoMapper.Add("permafrost keep", "permafrost caverns");
            ZoneWhoMapper.Add("clan runnyeye", "runnyeye citadel");
            ZoneWhoMapper.Add("the ruins of old paineel", "the hole");
            ZoneWhoMapper.Add("the warrens", "warrens");
            ZoneWhoMapper.Add("thurgadin", "city of thurgadin");
            ZoneWhoMapper.Add("upper guk", "guk");
            ZoneWhoMapper.Add("warsliks wood", "warsliks woods");
            ZoneWhoMapper.Add("west karana", "western plains of karana");
            ZoneWhoMapper.Add("clan crushbone", "crushbone");
            ZoneWhoMapper.Add("east karana", "eastern plains of karana");
            ZoneWhoMapper.Add("the field of bone", "field of bone");
            ZoneWhoMapper.Add("qeynos catacombs", "qeynos aqueduct system");
            ZoneWhoMapper.Add("felwithe", "northern felwithe");
            ZoneWhoMapper.Add("kaladim", "south kaladim");

            ZoneNameMapper.Add("ocean of tears", "oot");
            ZoneNameMapper.Add("northern plains of karana", "northkarana");
            ZoneNameMapper.Add("skyshrine", "skyshrine");
            ZoneNameMapper.Add("the nektulos forest", "nektulos");
            ZoneNameMapper.Add("sleepers tomb", "sleeper");
            ZoneNameMapper.Add("erudin", "erudnext");
            ZoneNameMapper.Add("kedge keep", "kedge");
            ZoneNameMapper.Add("ak'anon", "akanon");
            ZoneNameMapper.Add("warsliks woods", "warslikswood");
            ZoneNameMapper.Add("castle mistmoore", "mistmoore");
            ZoneNameMapper.Add("castle of mistmoore", "mistmoore");
            ZoneNameMapper.Add("high keep", "highkeep");
            ZoneNameMapper.Add("highpass hold", "highpass");
            ZoneNameMapper.Add("qeynos aqueduct system", "qcat");
            ZoneNameMapper.Add("lake of ill omen", "lakeofillomen");
            ZoneNameMapper.Add("kael drakkel", "kael");
            ZoneNameMapper.Add("tower of frozen shadow", "frozenshadow");
            ZoneNameMapper.Add("icewell keep", "thurgadinb");
            ZoneNameMapper.Add("the feerrott", "feerrott");
            ZoneNameMapper.Add("ruins of sebilis", "sebilis");
            ZoneNameMapper.Add("old sebilis", "sebilis");
            ZoneNameMapper.Add("east commonlands", "ecommons");
            ZoneNameMapper.Add("cabilis east", "cabeast");
            ZoneNameMapper.Add("veeshan's peak", "veeshan");
            ZoneNameMapper.Add("surefall glade", "qrg");
            ZoneNameMapper.Add("innothule swamp", "innothule");
            ZoneNameMapper.Add("halas", "halas");
            ZoneNameMapper.Add("solusek's eye", "soldunga");
            ZoneNameMapper.Add("estate of unrest", "unrest");
            ZoneNameMapper.Add("blackburrow", "blackburrow");
            ZoneNameMapper.Add("gorge of king xorbb", "beholder");
            ZoneNameMapper.Add("plane of hate", "hateplane");
            ZoneNameMapper.Add("plane of hate (Instance)", "hateplane");
            ZoneNameMapper.Add("west commonlands", "commons");
            ZoneNameMapper.Add("north qeynos", "qeynos2");
            ZoneNameMapper.Add("cobalt scar", "cobaltscar");
            ZoneNameMapper.Add("befallen", "befallen");
            ZoneNameMapper.Add("paineel", "paineel");
            ZoneNameMapper.Add("north freeport", "freportn");
            ZoneNameMapper.Add("nagafen's lair", "soldungb");
            ZoneNameMapper.Add("nagafen's lair (Instance)", "soldungb");
            ZoneNameMapper.Add("runnyeye citadel", "runnyeye");
            ZoneNameMapper.Add("frontier mountains", "frontiermtns");
            ZoneNameMapper.Add("the city of mist", "citymist");
            ZoneNameMapper.Add("west freeport", "freportw");
            ZoneNameMapper.Add("butcherblock mountains", "butcher");
            ZoneNameMapper.Add("permafrost caverns", "permafrost");
            ZoneNameMapper.Add("permafrost caverns (Instance)", "permafrost");
            ZoneNameMapper.Add("the hole", "hole");
            ZoneNameMapper.Add("qeynos hills", "qeytoqrg");
            ZoneNameMapper.Add("arena", "arena");
            ZoneNameMapper.Add("lavastorm mountains", "lavastorm");
            ZoneNameMapper.Add("plane of growth", "growthplane");
            ZoneNameMapper.Add("misty thicket", "misty");
            ZoneNameMapper.Add("city of thurgadin", "thurgadina");
            ZoneNameMapper.Add("northern desert of ro", "nro");
            ZoneNameMapper.Add("neriak foreign quarter", "neriaka");
            ZoneNameMapper.Add("neriak - foreign quarter", "neriaka");
            ZoneNameMapper.Add("infected paw", "paw");
            ZoneNameMapper.Add("lair of the splitpaw", "paw");
            ZoneNameMapper.Add("plane of air", "airplane");
            ZoneNameMapper.Add("plane of air (Instance)", "airplane");
            ZoneNameMapper.Add("southern felwithe", "felwitheb");
            ZoneNameMapper.Add("velketor's labyrinth", "velketor");
            ZoneNameMapper.Add("cabilis west", "cabwest");
            ZoneNameMapper.Add("lake rathetear", "lakerathe");
            ZoneNameMapper.Add("kurn's tower", "kurn");
            ZoneNameMapper.Add("dagnor's cauldron", "cauldron");
            ZoneNameMapper.Add("western wastes", "westwastes");
            ZoneNameMapper.Add("temple of veeshan", "templeveeshan");
            ZoneNameMapper.Add("lesser faydark", "lfaydark");
            ZoneNameMapper.Add("everfrost", "everfrost");
            ZoneNameMapper.Add("trakanon's teeth", "trakanon");
            ZoneNameMapper.Add("eastern plains of karana", "eastkarana");
            ZoneNameMapper.Add("north kaladim", "kaladimb");
            ZoneNameMapper.Add("dreadlands", "dreadlands");
            ZoneNameMapper.Add("south qeynos", "qeynos");
            ZoneNameMapper.Add("plane of fear", "fearplane");
            ZoneNameMapper.Add("plane of fear (Instance)", "fearplane");
            ZoneNameMapper.Add("rathe mountains", "rathemtn");
            ZoneNameMapper.Add("the wakening lands", "wakening");
            ZoneNameMapper.Add("southern desert of ro", "sro");
            ZoneNameMapper.Add("the burning wood", "burningwood");
            ZoneNameMapper.Add("greater faydark", "gfaydark");
            ZoneNameMapper.Add("dragon necropolis", "necropolis");
            ZoneNameMapper.Add("guk", "guktop");
            ZoneNameMapper.Add("the overthere", "overthere");
            ZoneNameMapper.Add("eastern wastelands", "eastwastes");
            ZoneNameMapper.Add("field of bone", "fieldofbone");
            ZoneNameMapper.Add("neriak third gate", "neriakc");
            ZoneNameMapper.Add("neriak - 3rd gate", "neriakc");
            ZoneNameMapper.Add("erud's crossing", "erudsxing");
            ZoneNameMapper.Add("northern felwithe", "felwithea");
            ZoneNameMapper.Add("firiona vie", "firiona");
            ZoneNameMapper.Add("east freeport", "freporte");
            ZoneNameMapper.Add("swamp of no hope", "swampofnohope");
            ZoneNameMapper.Add("timorous deep", "timorous");
            ZoneNameMapper.Add("dalnir", "dalnir");
            ZoneNameMapper.Add("southern plains of karana", "southkarana");
            ZoneNameMapper.Add("western plains of karana", "qey2hh1");
            ZoneNameMapper.Add("skyfire mountains", "skyfire");
            ZoneNameMapper.Add("mines of nurga", "nurga");
            ZoneNameMapper.Add("oasis of marr", "oasis");
            ZoneNameMapper.Add("the emerald jungle", "emeraldjungle");
            ZoneNameMapper.Add("great divide", "greatdivide");
            ZoneNameMapper.Add("sirens grotto", "sirens");
            ZoneNameMapper.Add("erudin palace", "erudnint");
            ZoneNameMapper.Add("toxxulia forest", "tox");
            ZoneNameMapper.Add("ruins of old guk", "gukbottom");
            ZoneNameMapper.Add("steamfont mountains", "steamfont");
            ZoneNameMapper.Add("south kaladim", "kaladima");
            ZoneNameMapper.Add("najena", "najena");
            ZoneNameMapper.Add("stonebrunt mountains", "stonebrunt");
            ZoneNameMapper.Add("howling stones", "charasis");
            ZoneNameMapper.Add("kerra isle", "kerraridge");
            ZoneNameMapper.Add("lost temple of cazic-thule", "cazicthule");
            ZoneNameMapper.Add("lost temple of cazicthule", "cazicthule");
            ZoneNameMapper.Add("neriak - commons", "neriakb");
            ZoneNameMapper.Add("neriak commons", "neriakb");
            ZoneNameMapper.Add("karnor's castle", "karnor");
            ZoneNameMapper.Add("crystal caverns", "crystal");
            ZoneNameMapper.Add("iceclad ocean", "iceclad");
            ZoneNameMapper.Add("warrens", "warrens");
            ZoneNameMapper.Add("oggok", "oggok");
            ZoneNameMapper.Add("grobb", "grobb");
            ZoneNameMapper.Add("rivervale", "rivervale");
            ZoneNameMapper.Add("plane of mischief", "mischiefplane");
            ZoneNameMapper.Add("kaesora", "kaesora");
            ZoneNameMapper.Add("temple of droga", "droga");
            ZoneNameMapper.Add("crushbone", "crushbone");
            ZoneNameMapper.Add("chardok", "chardok");
            ZoneNameMapper.Add("kithicor woods", "kithicor");
            ZoneNameMapper.Add("temple of solusek ro", "soltemple");
            Zones = ZoneInfoMap.Keys.ToList();
        }

        public static readonly List<string> Zones;
        public static string TranslateToMapName(string name)
        {
            name = name?.ToLower()?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (ZoneWhoMapper.TryGetValue(name, out var n))
            {
                name = n;
            }

            if (ZoneNameMapper.TryGetValue(name, out n))
            {
                name = n;
            }

            return Zones.Any(a => a == name) ? name : string.Empty;
        }

        public static string Match(string message)
        {
            //Debug.WriteLine($"ZoneParse: "+ message);
            if (message.StartsWith(Therearenoplayers) || message.StartsWith(Youhaveenteredareapvp))
            {
                return string.Empty;
            }
            else if (message.StartsWith(Youhaveentered))
            {
                message = message.Replace(Youhaveentered, string.Empty).Trim().TrimEnd('.').ToLower();
                return message;
            }
            else if (message.StartsWith(Thereare))
            {
                message = message.Replace(Thereare, string.Empty).Trim();
                var inindex = message.IndexOf(spaceinspace);
                if (inindex != -1)
                {
                    message = message.Substring(inindex + spaceinspace.Length).Trim().TrimEnd('.').ToLower();
                    if (message != "everquest")
                    {
                        return message;
                    }
                }
            }
            else if (message.StartsWith(Thereis))
            {
                message = message.Replace(Thereis, string.Empty).Trim();
                var inindex = message.IndexOf(spaceinspace);
                if (inindex != -1)
                {
                    message = message.Substring(inindex + spaceinspace.Length).Trim().TrimEnd('.').ToLower();
                    if (message != "everquest")
                    {
                        return message;
                    }
                }
            }

            return string.Empty;
        }
    }
}
