using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EQTool.Services.Map
{
    public static class ZoneParser
    {
        private const string Youhaveentered = "You have entered ";
        private const string Therearenoplayers = "There are no players in EverQuest that match those who filters.";
        private const string Thereare = "There are ";
        private const string spaceinspace = " in ";

        private static readonly Dictionary<string, string> ZoneNameMapper = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> ZoneWhoMapper = new Dictionary<string, string>();

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

        public static List<string> Zones => ZoneNameMapper.Keys.Concat(ZoneWhoMapper.Keys).Distinct().ToList();

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

        public static string Match(string linelog)
        {
            if (linelog == null || linelog.Length < 27)
            {
                return string.Empty;
            }

            var message = linelog.Substring(27);
            Debug.WriteLine($"ZoneParse: " + message);
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
