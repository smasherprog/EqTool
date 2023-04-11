using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Map
{
    public class ZoneInfo
    {
        public string Name { get; set; }
        public TimeSpan RespawnTime { get; set; }
        public List<string> NotableNPCs { get; set; } = new List<string>();

    }

    public static class ZoneParser
    {
        private const string Youhaveentered = "You have entered ";
        private const string Therearenoplayers = "There are no players ";
        private const string Thereare = "There are ";
        private const string spaceinspace = " in ";

        public static readonly Dictionary<string, string> ZoneNameMapper = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> ZoneWhoMapper = new Dictionary<string, string>();
        public static readonly Dictionary<string, bool> NoZoneZHelping = new Dictionary<string, bool>();
        public static readonly Dictionary<string, ZoneInfo> ZoneInfoMap = new Dictionary<string, ZoneInfo>();

        static ZoneParser()
        {
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

            ZoneInfoMap.Add("airplane", new ZoneInfo
            {
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
                NotableNPCs = new List<string>()
                {
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("arena", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                },
                RespawnTime = new TimeSpan(0)
            });
            ZoneInfoMap.Add("befallen", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                   "Boondin Babbinsbort","Commander Windstream","An Elf Skeleton","Gynok Moltor","Priest Amiaz","Skeleton Lrodd","The Thaumaturgist"
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("beholder", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "King Xorbb","Lord Syrkl","Lord Sviir","Lord Soptyvr","SpinFlint","Brahhm","Yymp the Infernal","Qlei","Goblin Alchemist"
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("blackburrow", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Lord Elgnub","Master Brewer","Refugee Splitpaw","a gnoll commander","Splitpaw Commander",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("burningwood", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("butcher", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("cabeast", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("cabwest", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("cauldron", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("cazicthule", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("charasis", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Sentient Bile","The Crypt Excavator","The Crypt Feaster","The Crypt Keeper","Drusella Sathir","Embalming Fluid","The Golem Master","Reanimated Plaguebone","Skeletal Procurator","The Skeleton Sepulcher","The Spectre Spiritualist","The Undertaker Lord",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("chardok", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("citymist", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "a black reaver","an army behemoth","Captain of the Guard","a human skeleton","Lhranc","Lord Ghiosk","Lord Rak`Ashiir","Neh`Ashiir","spectral courier","Wraith of Jaxion",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("cobaltscar", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("commons", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("crushbone", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Orc Taskmaster","Orc Trainer","Orc Warlord","Ambassador DVinn","Lord Darish","Rondo Dunfire","Retlon Brenclog","Emperor Crush","The Prophet",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("crystal", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("dalnir", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Coerced Channeler","Coerced Crusader","Coerced Penkeeper","a coerced revenant","a coerced smith","Kly Imprecator","The Kly Overseer","The Kly","Spectral Crusader","an undead blacksmith","lumpy goo",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("dreadlands", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Gorenaire","A dread widow","a mountain giant patriarch","a wulfare lonewolf","wraithbone champion",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("droga", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "a goblin bodyguard","Chief Rokgus","a goblin canyoneer","a maddened Burynai","Soothsayer Dregzak",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("eastkarana", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("eastwastes", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("ecommons", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("emeraldjungle", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("erudnext", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("erudnint", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("erudsxing", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("everfrost", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Karg IceBear","Lich of Miragul","Megan","Tundra Jack","Iceberg","Snowflake","Sulon McMoor","Redwind","Martar IceBear","Dark Assassin","Corrupted wooly",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("fearplane", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("feerrott", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Annaelia Wylassi","Aqaar Aluram","Cyndreela","Dark Assassin","Eleann Morkul","Oknoggin Stonesmacker","Roror","Spanner Scrapsnatcher",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("felwithea", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("felwitheb", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("fieldofbone", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "A scaled prowler","A skeletal jester","Burynaibane Spider","Carrion Queen","Gharg Oberbord","Iksar Dakoit","Jairnel Marfury","Kerosh Blackhand","Targishin","The Tangrin","a burynai cutter","a scourgetail scorpion",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("firiona", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("freporte", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("freportn", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("freportw", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("frontiermtns", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("frozenshadow", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Large Undead Gnoll","Xalgoti","Zorglim the Dead","Enraged Shadowbeast","Amontehepna","Narmak Berreka","maggot infested flesh","Eugie","Isopca","Lerty","Nosja","Otdd","Pelpa","Priest Majes Medory","Tihgren","Varjie","Vyakna","enraged relative","lucid spirit of Abrams","Vhal'Sera",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("gfaydark", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("greatdivide", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("grobb", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("growthplane", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("gukbottom", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("guktop", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "a froglok gaz squire","a froglok idealist","a froglok realist","a froglok necromancer","a froglok scryer","a froglok summoner","a froglok nokta shaman","a froglok shin knight","the froglok shin lord","Tempus","a giant heart spider",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("halas", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("hateplane", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("highkeep", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Captain Boshinko","Mistress Anna","Osargen","Princess Lenia",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("highpass", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Cyrla Shadowstepper","Dyllin Starsine","Hagnis Shralok","Recfek Shralok","Vopuk Shralok","Vexven Mucktail","Grenix Mucktail","Barn Bloodstone",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("hole", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("iceclad", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Balix Misteyes","Corudoth","Garou","Lodizal","Midnight","Pulsating Icestorm","Stormfeather",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("innothule", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Bunk Oden","Jojoojojgogogoguna","Jyle Windstorm","Jojongua","Zimbittle","a troll slayer","Spore Guardian","Lynuga",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kael", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Avatar of War","Statue of Rallos Zek","Derakor the Vindicator","King Tormax","Bjrakor the Cold","Captain Bvellos","Gkrean Prophet of Tallon","Semkak Prophet of Vallon","Gorul Longshanks","Keldor Dek`Torek","Noble Helssen","Slaggak the Trainer","Staff Sergeant Drioc","Vkjor","Wenglawks Kkeak",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kaesora", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Hungered Ravener","failed crypt raider","Frenzied Strathbone","Reaver of Xalgoz","spectral guardian","spectral librarian","Strathbone Runelord","tortured librarian","Warder of Xalgoz","Xalgoz",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kaladima", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kaladimb", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("karnor", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kedge", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Phinigel Autropos","Cauldronboil","Cauldronbubble","Coralyn Kelpmaiden","Estrella of Gloomwater","Fierce Impaler","a ferocious cauldron shark","Frenzied Cauldron Shark","Golden Haired Mermaid","Stiletto Fang Piranha","Seahorse Patriarch","Seahorse Matriarch","Shellara Ebbhunter","Undertow","Swirlspine",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kerraridge", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kithicor", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("kurn", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Bargynn","Burynai Forager","fingered skeleton","an odd mole","a skeletal cook","thick boned skeleton","undead crusader","an undead jester",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("lakeofillomen", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "a sarnak courier","Professor Akabao","Chancellor of Di`Zok","Lord Gorelik","Advisor Sh'Orok",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("lakerathe", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("lavastorm", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Deep Lava Basilisk","Eejag","Hykallen","A lesser nightmare","Sir Lindeal","a warbone monk","a warbone spearman",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("lfaydark", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("mischiefplane", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("mistmoore", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "an advisor","an avenging caitiff","Black Dire","Butler Syncall","a cloaked dhampyre","a deathly usher","Enynti","Garton Viswin","a glyphed ghoul","an imp familiar","Lasna Cheroon","Maid Issis","Mayong Mistmoore","Mynthi Davissi","Princess Cherista","Ssynthi","Xicotl",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("misty", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("najena", new ZoneInfo
            {
                NotableNPCs = new List<string>()
                {
                    "Akksstaff","BoneCracker","Drelzna","Ekeros","Linara Parlone","Moosh","Najena (NPC)","Officer Grush","Rathyl","Rathyl reincarnate","Trazdon","a visiting priestess","The Widowmistress",
                },
                RespawnTime = new TimeSpan(0, 6, 40)
            });
            ZoneInfoMap.Add("necropolis", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("nektulos", new ZoneInfo { NotableNPCs = new List<string>() { "Leatherfoot Deputy", "Leatherfoot Medic", "Kirak Vil", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("neriaka", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("neriakb", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("neriakc", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("northkarana", new ZoneInfo { NotableNPCs = new List<string>() { "Xanuusus", "Ashenpaw", "Zahal the Vile", "GrimFeather", "Swiftclaw", "Lieutenant Midraim", "The Silver Griffon", "Timbur the Tiny", "Korvik the Cursed", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("nro", new ZoneInfo { NotableNPCs = new List<string>() { "Dorn B`Dynn", "Dunedigger", "Rahotep", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("nurga", new ZoneInfo { NotableNPCs = new List<string>() { "Overseer Dlubish", "A Sleeping Ogre", "Trunt", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("oasis", new ZoneInfo { NotableNPCs = new List<string>() { "Cazel", "Hatar", "Lockjaw", "Young Ronin", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("oggok", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("oot", new ZoneInfo { NotableNPCs = new List<string>() { "Capt Surestout", "Nerbilik", "Oracle of K`Arnon", "Boog Mudtoe", "Gornit", "Sentry Xyrin", "Gull Skytalon", "Allizewsaur", "Ancient Cyclops", "Brawn", "Quag Maelstrom", "Seplawishinl Bladeblight", "Soarin Brightfeather", "tainted seafury cyclops", "corrupted seafury cyclops", "Wiltin Windwalker", "A Goblin", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("overthere", new ZoneInfo { NotableNPCs = new List<string>() { "Admiral Tylix", "Captain Rottgrime", "General V`Deers", "Impaler Tzilug", "Tourmaline", "Corundium", "Stishovite", "Tektite", "A Cliff Golem", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("paineel", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("paw", new ZoneInfo { NotableNPCs = new List<string>() { "Brother Hayle", "The Ishva Mal", "Kurrpok Splitpaw", "Tesch Val Kadvem", "Tesch Val Deval`Nmak", "Nisch Val Torash Mashk", "Rosch Val L'Vlor" }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("permafrost", new ZoneInfo { NotableNPCs = new List<string>() { "Lady Vox", "Priest of Nagafen", "High Priest Zaharn", "A goblin alchemist (Permafrost)", "King Thex'Ka IV", "Goblin Archeologist", "Goblin Patriarch", "Goblin Preacher", "Goblin Jail Master", "Goblin Scryer", "Elite Honor Guard", "Injured Polar Bear", "Ice Goblin Champion", "Ice Giant Diplomat", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("qcat", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("qey2hh1", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("qeynos", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("qeynos2", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("qeytoqrg", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("qrg", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("rathemtn", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("rivervale", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("runnyeye", new ZoneInfo { NotableNPCs = new List<string>() { "Borxx", "an Evil Eye prisoner", "Sludge Dankmire", "A Goblin Captain", "Goblin Warlord", "The Goblin King", "Slime Elemental", "Gelatinous Cube", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("sebilis", new ZoneInfo { NotableNPCs = new List<string>() { "Arch Duke Iatol", "Baron Yosig", "blood of chottal", "Brogg", "crypt caretaker", "Emperor Chottal", "frenzied pox scarab", "Froggy", "froglok armorer", "froglok armsman", "froglok chef", "froglok commander", "froglok ostiary", "froglok pickler", "froglok repairer", "Gangrenous scarab", "Gruplinort", "Harbinger Freglor", "Hierophant Prime Grekal", "myconid spore king", "a necrosis scarab", "sebilite protector", "Tolapumj", "Trakanon", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("sirens", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("skyfire", new ZoneInfo { NotableNPCs = new List<string>() { "Black Scar", "Eldrig the Old", "Faerie of Dismay", "Felia Goldenwing", "Guardian of Felia", "Jennus Lyklobar", "a lava walker", "a shadow drake", "a soul devourer", "Talendor", "a wandering wurm", "a wurm spirit", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("skyshrine", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("sleeper", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("soldunga", new ZoneInfo { NotableNPCs = new List<string>() { "Captain Bipnubble", "CWG Model EXG", "Fire Goblin Bartender", "Inferno Goblin Captain", "Fire Goblin Drunkard", "Goblin High Shaman", "Solusek Goblin King", "Gabbie Mardoddle", "flame goblin foreman", "Inferno Goblin Torturer", "Kindle", "Kobold predator", "lava elemental", "Lord Gimblox", "Lynada the Exiled", "Marfen Binkdirple", "Reckless Efreeti", "Singe", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("soldungb", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("soltemple", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("southkarana", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("sro", new ZoneInfo { NotableNPCs = new List<string>() { "Ancient Cyclops", "Erg Bluntbruiser", "Ortallius", "Rathmana Allin", "Sandgiant Husam", "Scrounge", "Terrorantula", "Young Ronin", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("steamfont", new ZoneInfo { NotableNPCs = new List<string>() { "Feddi Dooger", "A Kobold Missionary", "Meldrath The Malignant", "Minotaur Hero", "Minotaur Lord", "Renux Herkanor", "Nilit's contraption", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("stonebrunt", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("swampofnohope", new ZoneInfo { NotableNPCs = new List<string>() { "Bloodgorge", "an escaped froglok", "Deadeye", "Dreesix Ghoultongue", "Dugroz", "Fakraa the Forsaken", "Fangor", "Frayk", "Froglok Repairer", "Froszik the Impaler", "Grik the Exiled", "Grimewurm", "Grizshnok", "Soblohg", "Two Tails", "Ulump Pujluk", "Venomwing", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("templeveeshan", new ZoneInfo { NotableNPCs = new List<string>() { "Casalen", "Dozekar the Cursed", "Essedera", "Grozzmel", "Krigara", "Lepethida", "Midayor", "Tavekalem", "Ymmeln", "Gozzrem", "Lendiniara the Keeper", "Telkorenar", "Cekenar", "Dagarn the Destroyer", "Eashen of the Sky", "Ikatiar the Venom", "Jorlleag", "Lady Mirenilla", "Lady Nevederia", "Lord Feshlak", "Lord Koi'Doken", "Lord Kreizenn", "Lord Vyemm", "Sevalak", "Vulak`Aerr", "Zlexak", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("thurgadina", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("thurgadinb", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("timorous", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("tox", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("trakanon", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("unrest", new ZoneInfo { NotableNPCs = new List<string>() { "Garanel Rucksif", "a priest of najena", "Khrix Fritchoff", "Khrix's Abomination", "Torklar Battlemaster", "Shadowpincer", "reclusive ghoul magus", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("veeshan", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("velketor", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("wakening", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("warrens", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("warslikswood", new ZoneInfo { NotableNPCs = new List<string>() { "", }, RespawnTime = new TimeSpan(0, 6, 40) });
            ZoneInfoMap.Add("westwastes", new ZoneInfo { NotableNPCs = new List<string>() { "Atpaev", "Ayillish", "Bratavar", "Bufa", "Cargalia", "Del Sapara", "Derasinal", "Draazak", "Entariz", "Esorpa of the Ring", "Gafala", "Gangel", "Glati", "Harla Dar", "Hechaeva", "Honvar", "Ionat", "Jen Sapara", "Kar Sapara", "Karkona", "Klandicar", "Linbrak", "Makala", "Mazi", "Melalafen", "Myga", "Neordla", "Nintal", "Onava", "Pantrilla", "Quoza", "Sivar", "Sontalak", "Uiliak", "Vitaela", "Von", "Vraptin", "Yal", "Yeldema", "Zil Sapara", "Icehackle", "Makil Rargon", "Mraaka", "Scout Charisa", "Strong Horn", "Tantor", "The Dragon Sage", "Tranala", "Tsiraka", }, RespawnTime = new TimeSpan(0, 6, 40) });





            NoZoneZHelping.Add("airplane", false);
            NoZoneZHelping.Add("akanon", false);
            NoZoneZHelping.Add("arena", false);
            NoZoneZHelping.Add("befallen", false);
            NoZoneZHelping.Add("beholder", false);
            NoZoneZHelping.Add("blackburrow", true);
            NoZoneZHelping.Add("burningwood", false);
            NoZoneZHelping.Add("butcher", false);
            NoZoneZHelping.Add("cabeast", false);
            NoZoneZHelping.Add("cabwest", false);
            NoZoneZHelping.Add("cauldron", false);
            NoZoneZHelping.Add("cazicthule", false);
            NoZoneZHelping.Add("charasis", false);
            NoZoneZHelping.Add("chardok", true);
            NoZoneZHelping.Add("citymist", true);
            NoZoneZHelping.Add("cobaltscar", false);
            NoZoneZHelping.Add("commons", false);
            NoZoneZHelping.Add("crushbone", false);
            NoZoneZHelping.Add("crystal", false);
            NoZoneZHelping.Add("dalnir", true);
            NoZoneZHelping.Add("dreadlands", false);
            NoZoneZHelping.Add("droga", false);
            NoZoneZHelping.Add("eastkarana", false);
            NoZoneZHelping.Add("eastwastes", false);
            NoZoneZHelping.Add("ecommons", false);
            NoZoneZHelping.Add("emeraldjungle", false);
            NoZoneZHelping.Add("erudnext", false);
            NoZoneZHelping.Add("erudnint", true);
            NoZoneZHelping.Add("erudsxing", false);
            NoZoneZHelping.Add("everfrost", false);
            NoZoneZHelping.Add("fearplane", false);
            NoZoneZHelping.Add("feerrott", false);
            NoZoneZHelping.Add("felwithea", true);
            NoZoneZHelping.Add("felwitheb", false);
            NoZoneZHelping.Add("fieldofbone", false);
            NoZoneZHelping.Add("firiona", false);
            NoZoneZHelping.Add("freporte", true);
            NoZoneZHelping.Add("freportn", false);
            NoZoneZHelping.Add("freportw", false);
            NoZoneZHelping.Add("frontiermtns", false);
            NoZoneZHelping.Add("frozenshadow", false);
            NoZoneZHelping.Add("gfaydark", false);
            NoZoneZHelping.Add("greatdivide", false);
            NoZoneZHelping.Add("grobb", false);
            NoZoneZHelping.Add("growthplane", false);
            NoZoneZHelping.Add("gukbottom", true);
            NoZoneZHelping.Add("guktop", true);
            NoZoneZHelping.Add("halas", false);
            NoZoneZHelping.Add("hateplane", true);
            NoZoneZHelping.Add("highkeep", true);
            NoZoneZHelping.Add("highpass", false);
            NoZoneZHelping.Add("hole", true);
            NoZoneZHelping.Add("iceclad", false);
            NoZoneZHelping.Add("innothule", false);
            NoZoneZHelping.Add("kael", false);
            NoZoneZHelping.Add("kaesora", true);
            NoZoneZHelping.Add("kaladima", false);
            NoZoneZHelping.Add("kaladimb", false);
            NoZoneZHelping.Add("karnor", true);
            NoZoneZHelping.Add("kedge", true);
            NoZoneZHelping.Add("kerraridge", false);
            NoZoneZHelping.Add("kithicor", false);
            NoZoneZHelping.Add("kurn", false);
            NoZoneZHelping.Add("lakeofillomen", false);
            NoZoneZHelping.Add("lakerathe", false);
            NoZoneZHelping.Add("lavastorm", false);
            NoZoneZHelping.Add("lfaydark", false);
            NoZoneZHelping.Add("mischiefplane", true);
            NoZoneZHelping.Add("mistmoore", true);
            NoZoneZHelping.Add("misty", false);
            NoZoneZHelping.Add("najena", true);
            NoZoneZHelping.Add("necropolis", false);
            NoZoneZHelping.Add("nektulos", false);
            NoZoneZHelping.Add("neriaka", false);
            NoZoneZHelping.Add("neriakb", false);
            NoZoneZHelping.Add("neriakc", false);
            NoZoneZHelping.Add("northkarana", false);
            NoZoneZHelping.Add("nro", false);
            NoZoneZHelping.Add("nurga", false);
            NoZoneZHelping.Add("oasis", false);
            NoZoneZHelping.Add("oggok", false);
            NoZoneZHelping.Add("oot", false);
            NoZoneZHelping.Add("overthere", false);
            NoZoneZHelping.Add("paineel", false);
            NoZoneZHelping.Add("paw", true);
            NoZoneZHelping.Add("permafrost", true);
            NoZoneZHelping.Add("qcat", true);
            NoZoneZHelping.Add("qey2hh1", false);
            NoZoneZHelping.Add("qeynos", true);
            NoZoneZHelping.Add("qeynos2", false);
            NoZoneZHelping.Add("qeytoqrg", false);
            NoZoneZHelping.Add("qrg", true);
            NoZoneZHelping.Add("rathemtn", false);
            NoZoneZHelping.Add("rivervale", false);
            NoZoneZHelping.Add("runnyeye", true);
            NoZoneZHelping.Add("sebilis", true);
            NoZoneZHelping.Add("sirens", true);
            NoZoneZHelping.Add("skyfire", false);
            NoZoneZHelping.Add("skyshrine", true);
            NoZoneZHelping.Add("sleeper", false);
            NoZoneZHelping.Add("soldunga", true);
            NoZoneZHelping.Add("soldungb", true);
            NoZoneZHelping.Add("soltemple", true);
            NoZoneZHelping.Add("southkarana", false);
            NoZoneZHelping.Add("sro", false);
            NoZoneZHelping.Add("steamfont", false);
            NoZoneZHelping.Add("stonebrunt", false);
            NoZoneZHelping.Add("swampofnohope", false);
            NoZoneZHelping.Add("templeveeshan", false);
            NoZoneZHelping.Add("thurgadina", false);
            NoZoneZHelping.Add("thurgadinb", true);
            NoZoneZHelping.Add("timorous", false);
            NoZoneZHelping.Add("tox", false);
            NoZoneZHelping.Add("trakanon", false);
            NoZoneZHelping.Add("unrest", true);
            NoZoneZHelping.Add("veeshan", false);
            NoZoneZHelping.Add("velketor", true);
            NoZoneZHelping.Add("wakening", false);
            NoZoneZHelping.Add("warrens", false);
            NoZoneZHelping.Add("warslikswood", false);
            NoZoneZHelping.Add("westwastes", false);

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
            ZoneNameMapper.Add("west commonlands", "commons");
            ZoneNameMapper.Add("north qeynos", "qeynos2");
            ZoneNameMapper.Add("cobalt scar", "cobaltscar");
            ZoneNameMapper.Add("befallen", "befallen");
            ZoneNameMapper.Add("paineel", "paineel");
            ZoneNameMapper.Add("north freeport", "freportn");
            ZoneNameMapper.Add("nagafen's lair", "soldungb");
            ZoneNameMapper.Add("runnyeye citadel", "runnyeye");
            ZoneNameMapper.Add("frontier mountains", "frontiermtns");
            ZoneNameMapper.Add("the city of mist", "citymist");
            ZoneNameMapper.Add("west freeport", "freportw");
            ZoneNameMapper.Add("butcherblock mountains", "butcher");
            ZoneNameMapper.Add("permafrost caverns", "permafrost");
            ZoneNameMapper.Add("the hole", "hole");
            ZoneNameMapper.Add("qeynos hills", "qeytoqrg");
            ZoneNameMapper.Add("arena", "arena");
            ZoneNameMapper.Add("lavastorm mountains", "lavastorm");
            ZoneNameMapper.Add("plane of growth", "growthplane");
            ZoneNameMapper.Add("misty thicket", "misty");
            ZoneNameMapper.Add("city of thurgadin", "thurgadina");
            ZoneNameMapper.Add("northern desert of ro", "nro");
            ZoneNameMapper.Add("neriak foreign quarter", "neriaka");
            ZoneNameMapper.Add("infected paw", "paw");
            ZoneNameMapper.Add("plane of air", "airplane");
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
        }

        public static List<string> Zones => NoZoneZHelping.Keys.ToList();

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

            return ZoneNameMapper.TryGetValue(name, out n) ? n : name;
        }

        public static string Match(string message)
        {
            //Debug.WriteLine($"ZoneParse: " + message);
            if (message.StartsWith(Therearenoplayers))
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
                    return message;
                }
            }

            return string.Empty;
        }
    }
}
